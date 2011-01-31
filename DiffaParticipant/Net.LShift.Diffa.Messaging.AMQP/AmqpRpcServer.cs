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

using RabbitMQ.Client.MessagePatterns.Unicast;

using Net.LShift.Diffa.Messaging.AMQP;

namespace Net.LShift.Diffa.Messaging.Amqp
{
    public class AmqpRpcServer : IDisposable
    {
        private readonly IMessaging _messaging;
        private readonly IJsonRpcHandler _handler;
        private Thread _worker;
        private bool _disposing;

        public AmqpRpcServer(IConnector connector, string queueName, IJsonRpcHandler handler)
        {
            _messaging = CreateMessaging(connector, queueName);
            _handler = handler;
            _worker = new Thread(WorkerLoop);
        }

        /// <summary>
        /// Initialize the server's Messaging and start its worker thread
        /// </summary>
        public void Start()
        {
            _messaging.Init();
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
                return _messaging.Receive(100);
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
            var headers = new Dictionary<string, object>
            {
                {AmqpRpc.EndpointHeader, request.Endpoint},
                {AmqpRpc.StatusCodeHeader, response.Status}
            };
            reply.Properties.Headers = headers;
            reply.Body = Serialize(response.Content);
            _messaging.Send(reply);
        }

        private void ReceiveHandleAckReply()
        {
            var message = Receive();
            var request = new JsonTransportRequest(EndpointFor(message), Deserialize(message.Body));
            var response = _handler.HandleRequest(request);
            Ack(message);
            Reply(message, request, response);
        }

        private static IMessaging CreateMessaging(IConnector connector, string queueName)
        {
            var messaging = Factory.CreateMessaging();
            messaging.Connector = connector;
            messaging.QueueName = queueName;
            messaging.SetupReceiver += channel => channel.QueueDeclare(queueName);
            return messaging;
        }

        internal class AmqpException : Exception {}

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
                    ReceiveHandleAckReply();
                }
                catch (AmqpException)
                {
                    // just do nothing on this iteration
                }
            }
        }

        private static byte[] Serialize(JObject obj)
        {
            var sw = new StringWriter();
            var writer = new JsonTextWriter(sw);
            obj.WriteTo(writer);
            return Encoding.UTF8.GetBytes(sw.ToString());
        }

        private static JObject Deserialize(byte[] data)
        {
            return JObject.Parse(Encoding.UTF8.GetString(data));
        }

        private static String EndpointFor(IReceivedMessage message)
        {
            var headers = message.Properties.Headers;
            var endpointHeader = headers[AmqpRpc.EndpointHeader];
            return endpointHeader.ToString();
        }

    }

    internal class AmqpRpc
    {
        public const String Encoding = "UTF-8";
        public const String EndpointHeader = "rpc-endpoint";
        public const String StatusCodeHeader = "rpc-status-code";
        public const int DefaultStatusCode = 200;
    }
}
