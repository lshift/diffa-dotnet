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

        public JsonAmqpRpcClient(string hostName, String target)
        {
            _target = target;
            _messaging = AmqpRpc.CreateMessaging(AmqpRpc.CreateConnector(hostName), "QUEUE_NAME");
            _messaging.SetupReceiver += channel =>
            {
                _replyTo = channel.QueueDeclare();
                _messaging.QueueName = _replyTo;
            };
            _messaging.QueueName = "DUMMY_QUEUE_NAME";
            _messaging.Init();
        }

        public JContainer Call(String endpoint, JContainer body)
        {
            var message = _messaging.CreateMessage();
            message.To = _target;
            message.ReplyTo = _replyTo;
            message.Body = Json.Serialize(body);
            message.Properties.Headers = AmqpRpc.CreateHeaders(endpoint, 200);
            _messaging.Send(message);

            var reply = _messaging.Receive(2000);
            // TODO handle null reply (timeout)
            // TODO handle error codes in headers
            return Json.Deserialize(reply.Body);
        }

        public void Dispose()
        {
            if (_messaging == null)
            {
                return;
            }
            _messaging.Cancel();
            _messaging = null;
        }
    }
}
