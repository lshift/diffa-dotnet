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

using NUnit.Framework;
using Rhino.Mocks;

using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns.Configuration;
using RabbitMQ.Client.MessagePatterns.Unicast;

using Net.LShift.Diffa.Messaging.Amqp;

namespace Net.LShift.Diffa.Messaging.AMQP.Test
{
    [TestFixture]
    public class AmqpEndToEndTest
    {

        private MockRepository mockery = new MockRepository();

        [Test]
        public void ServerCanStartAndDispose()
        {
            // Simple smoke test for the worker thread disposal; just starts and then disposes the server.
            // TODO this assumes Rabbit is running on the named endpoint; could boot up a Rabbit instance on localhost instead
            var connectionBuilder = new ConnectionBuilder(new ConnectionFactory(), new AmqpTcpEndpoint("mrnoisy.lshift.net"));
            var connector = Factory.CreateConnector(connectionBuilder);
            var handler = mockery.StrictMock<IJsonRpcHandler>();
            using (var server = new AmqpRpcServer(connector, "DUMMY_QUEUE_NAME", handler))
            {
                server.Start();
            }
        }

    }
}
