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

using NUnit.Framework;
using Rhino.Mocks;

using Newtonsoft.Json.Linq;

using Net.LShift.Diffa.Participants;

namespace Net.LShift.Diffa.Messaging.Amqp.Test
{
    [TestFixture]
    public class AmqpEndToEndTest
    {

        private readonly MockRepository _mockery = new MockRepository();

        [Test]
        public void ServerCanStartAndDispose()
        {
            // Simple smoke test for the worker thread disposal; just starts and then disposes the server.
            var participant = _mockery.StrictMock<IParticipant>();
            using (var server = new JsonAmqpRpcServer("localhost", "DUMMY_QUEUE_NAME",
                new ParticipantHandler(participant)))
            {
                server.Start();
            }
        }

        [Test]
        public void ServerShouldRespondToRpc()
        {
            var participant = new StubParticipant();
            
            using (var client = new JsonAmqpRpcClient("localhost", "QUEUE_NAME"))
            {
                using (var server = new JsonAmqpRpcServer("localhost", "QUEUE_NAME", new ParticipantHandler(participant)))
                {
                    server.Start();
                    var response = client.Call("query_aggregate_digests", JObject.Parse(@"{""constraints"": [], ""buckets"": {}}"));
                    var expectedResponse = JArray.Parse(@"[{""attributes"": [""2011-01""],
                                                            ""metadata"": {""lastUpdated"": ""2011-01-31T16:22:23.7240000Z"",
                                                                           ""digest"": ""4dac11f9c09f3ebc8842790cd5dec24a""}}]");
                    Assert.AreEqual(expectedResponse.ToString(), response.ToString());
                }
            }

        }

        internal class StubParticipant : IParticipant
        {
            public QueryAggregateDigestsResponse QueryAggregateDigests(QueryAggregateDigestsRequest request)
            {
                return new QueryAggregateDigestsResponse(new List<AggregateDigest> {
                    new AggregateDigest(new List<string> { "2011-01" }, new DateTime(2011, 01, 31, 16, 22, 23, 724),
                    "4dac11f9c09f3ebc8842790cd5dec24a") });
            }
        }

    }
}
