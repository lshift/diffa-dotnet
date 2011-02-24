//
//  Copyright (C) 2011 LShift Ltd.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using RabbitMQ.Client.MessagePatterns.Unicast;

namespace Net.LShift.Diffa.Messaging.Amqp
{
    /// <summary>
    /// AMQP RPC server which listens to an exclusive queue and responds to JSON-encoded requests
    /// </summary>
    public class JsonAmqpRpcServer : IDisposable
    {
        private readonly IMessaging _messaging;
        private readonly IJsonRpcHandler _handler;
        private Thread _worker;
        private bool _disposing;

        private readonly Logger _log;

        public JsonAmqpRpcServer(string hostName, string queueName, IJsonRpcHandler handler)
        {
            _log = LogManager.GetCurrentClassLogger();
            _log.Info("AMQP RPC server starting");
            _messaging = AmqpRpc.CreateMessaging(AmqpRpc.CreateConnector(hostName), queueName);
            _handler = handler;
        }

        public JsonAmqpRpcServer(string hostName, string queueName, IJsonRpcHandler handler, Logger logger)
        {
            _log = logger;
            _log.Info("AMQP RPC server starting");
            _messaging = AmqpRpc.CreateMessaging(AmqpRpc.CreateConnector(hostName), queueName);
            _handler = handler;
        }

        /// <summary>
        /// Initialize the server's Messaging and start its worker thread
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                if (_worker != null)
                {
                    throw new InvalidOperationException("Server is already running");
                }
            }
            _messaging.Init();
            _worker = new Thread(WorkerLoop);
            _worker.Start();
        }

        /// <summary>
        /// Attempt to stop the worker thread gracefully, giving it time to finish handling and acking a message.
        /// After a certain timeout, give up and dispose of the worker thread forcefully.
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                if (_disposing)
                {
                    return;
                }
                _disposing = true;
            }
            _messaging.Cancel();
            if (_worker != null)
            {
                _worker.Join(2000);
                _messaging.Dispose();
                _worker.Join(2000);
                _worker = null;
            }
        }

        private IReceivedMessage Receive()
        {
            try
            {
                var message = _messaging.Receive(100);
                return message;
            }
            catch (EndOfStreamException)
            {
                throw new AmqpException();
            }
        }

        private void Ack(IReceivedMessage message)
        {
            _messaging.Ack(message);
        }

        private void Reply(IReceivedMessage message, JsonTransportRequest request, JsonTransportResponse response)
        {
            var reply = message.CreateReply();
            var headers = AmqpRpc.CreateHeaders(request.Endpoint, response.Status);
            reply.Properties.Headers = headers;
            _log.Debug("Sending reply: " + response.Body);
            reply.Body = Json.Serialize(response.Body);
            reply.From = null;
            _messaging.Send(reply);
        }

        private void ReceiveAckHandleReply()
        {
            var message = Receive();
            if (message == null)
            {
                return;
            }
            Ack(message);
            var messageBody = Json.Deserialize(message.Body);
            _log.Debug("Received message: " + messageBody);
            var request = new JsonTransportRequest(EndpointFor(message), messageBody);
            try
            {
                var response = _handler.HandleRequest(request);
                Reply(message, request, response);
            }
            catch (Exception e)
            {
                var response = JsonTransportResponse.Error(e.Message);
                Reply(message, request, response);
                throw;
            }
        }

        internal class AmqpException : Exception
        {
        }

        private void WorkerLoop()
        {
            while (true)
            {
                lock (this)
                {
                    if (_disposing)
                    {
                        return;
                    }
                }
                try
                {
                    ReceiveAckHandleReply();
                }
                catch (AmqpException)
                {
                    // just do nothing on this iteration
                }
            }
        }

        private static String EndpointFor(IReceivedMessage message)
        {
            var headers = message.Properties.Headers;
            if (! headers.Contains(AmqpRpc.EndpointHeader))
            {
                throw new KeyNotFoundException("Message did not contain header: " + AmqpRpc.EndpointHeader);
            }
            var endpointHeader = headers[AmqpRpc.EndpointHeader];
            return Encoding.UTF8.GetString((byte[]) endpointHeader);
        }

    }

    /// <summary>
    /// Utilities for serializing/deserializing the Newtonsoft JSON-related types
    /// </summary>
    public class Json
    {
        public static byte[] Serialize(JContainer item)
        {
            var stringWriter = new StringWriter();
            var writer = new JsonTextWriter(stringWriter);
            item.WriteTo(writer);
            return Encoding.UTF8.GetBytes(stringWriter.ToString());
        }

        public static JContainer Deserialize(byte[] data)
        {
            var decodedString = Encoding.UTF8.GetString(data);
            try
            {
                return JObject.Parse(decodedString);
            }
            catch (Exception)
            {
                return JArray.Parse(decodedString);
            }
        }

    }
}
