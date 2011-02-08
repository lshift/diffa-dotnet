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
using System.Text;
using Newtonsoft.Json.Linq;

using RabbitMQ.Client.MessagePatterns.Unicast;

namespace Net.LShift.Diffa.Messaging.Amqp
{
    /// <summary>
    /// Not required for the creation of a participant; useful mainly for testing
    /// </summary>
    public class JsonAmqpRpcClient : IDisposable
    {
        private IMessaging _messaging;
        private string _replyTo;

        private readonly string _target;

        public JsonAmqpRpcClient(string hostName, string target)
        {
            _target = target;
            var connector = AmqpRpc.CreateConnector(hostName);
            _messaging = Factory.CreateMessaging();
            _messaging.Connector = connector;
            _messaging.SetupReceiver += channel =>
            {
                _replyTo = channel.QueueDeclare();
                _messaging.QueueName = _replyTo;
            };
            _messaging.QueueName = "DUMMY_QUEUE_NAME";
            _messaging.Init();
        }

        public JContainer Call(string endpoint, JContainer body)
        {
            return Call(endpoint, body, 60000);
        }

        public JContainer Call(string endpoint, JContainer body, int receiveTimeout)
        {
            var message = _messaging.CreateMessage();
            message.To = _target;
            message.ReplyTo = _replyTo;
            message.Body = Json.Serialize(body);
            message.Properties.Headers = AmqpRpc.CreateHeaders(endpoint, null);
            _messaging.Send(message);

            var reply = _messaging.Receive(receiveTimeout);
            if (AmqpRpc.GetStatusCode(reply) != 200)
            {
                throw new AmqpRpcError(Encoding.UTF8.GetString(reply.Body));
            }
            // TODO handle null reply (timeout)

            return Json.Deserialize(reply.Body);
        }

        public void Dispose()
        {
            _messaging.Cancel();
            _messaging.Dispose();
        }
    }


    public class AmqpRpcError : Exception
    {
        public AmqpRpcError(string message)
            : base(message)
        {
        }
    }
}
