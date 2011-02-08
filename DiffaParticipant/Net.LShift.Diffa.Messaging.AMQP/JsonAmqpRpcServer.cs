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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns.Configuration;
using RabbitMQ.Client.MessagePatterns.Unicast;

namespace Net.LShift.Diffa.Messaging.Amqp
{
    public class JsonAmqpRpcServer : IDisposable
    {
        private readonly IMessaging _messaging;
        private readonly IJsonRpcHandler _handler;
        private Thread _worker;
        private bool _disposing;

        public JsonAmqpRpcServer(string hostName, string queueName, IJsonRpcHandler handler)
        {
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
                if (message == null)
                {
                    // TODO handle this more appropriately
                    throw new AmqpException();
                }
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
            reply.Body = Json.Serialize(response.Body);
            reply.From = null;
            _messaging.Send(reply);
        }

        private void ReceiveAckHandleReply()
        {
            var message = Receive();
            Ack(message);
            var request = new JsonTransportRequest(EndpointFor(message), Json.Deserialize(message.Body));
            try
            {
                var response = _handler.HandleRequest(request);
                Reply(message, request, response);
            }
            catch (Exception e)
            {
                var response = new JsonTransportResponse(500, JArray.Parse(@"[{""error"": """+e.Message+@"""}]"));
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
            var endpointHeader = headers[AmqpRpc.EndpointHeader];
            return Encoding.UTF8.GetString((byte[]) endpointHeader);
        }

        private static EventLog CreateEventLog()
        {
            var eventLogSource = typeof(JsonAmqpRpcServer).ToString();
            if (!EventLog.SourceExists(eventLogSource))
            {
                EventLog.CreateEventSource(eventLogSource, "Application");
            }
            return new EventLog { Source = eventLogSource };
        }

    }

    internal class AmqpRpc
    {
        public const String Encoding = "UTF-8";
        public const String EndpointHeader = "rpc-endpoint";
        public const String StatusCodeHeader = "rpc-status-code";
        public const int DefaultStatusCode = 200;

        internal static IConnector CreateConnector(String hostName)
        {
            var connectionBuilder = new ConnectionBuilder(new ConnectionFactory(), new AmqpTcpEndpoint(hostName));
            return Factory.CreateConnector(connectionBuilder);
        }

        internal static IMessaging CreateMessaging(IConnector connector, string queueName)
        {
            var messaging = Factory.CreateMessaging();
            messaging.Connector = connector;
            messaging.QueueName = queueName;
            messaging.SetupReceiver += channel => channel.QueueDeclare(queueName);
            return messaging;
        }

        internal static IDictionary CreateHeaders(String endpoint, int? status)
        {
            return new Dictionary<string, object>
            {
                {EndpointHeader, endpoint},
                {StatusCodeHeader, status}
            };
        }

        internal static int? GetStatusCode(IMessage message)
        {
            var replyHeaders = message.Properties.Headers;
            if (! replyHeaders.Contains(StatusCodeHeader))
            {
                return null;
            }
            return (int) replyHeaders[StatusCodeHeader];
        }
    }

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
