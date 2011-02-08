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

using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns.Configuration;
using RabbitMQ.Client.MessagePatterns.Unicast;

namespace Net.LShift.Diffa.Messaging.Amqp
{
    /// <summary>
    /// Utilities for the AMQP RPC server and client
    /// </summary>
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
            if (!replyHeaders.Contains(StatusCodeHeader))
            {
                return null;
            }
            return (int)replyHeaders[StatusCodeHeader];
        }
    }
}
