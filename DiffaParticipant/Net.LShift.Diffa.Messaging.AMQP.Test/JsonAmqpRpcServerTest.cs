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
    public class JsonAmqpRpcServerTest
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
        public void ServerShouldRespondToQueryAggregateDigests()
        {
            var participant = new StubParticipant();
            
            using (var client = new JsonAmqpRpcClient("localhost", "QUEUE_NAME"))
            {
                using (var server = new JsonAmqpRpcServer("localhost", "QUEUE_NAME", new ParticipantHandler(participant)))
                {
                    server.Start();
                    var response = client.Call("query_aggregate_digests", JObject.Parse(@"{""constraints"": [], ""buckets"": {}}"));
                    var expectedResponse = JArray.Parse(@"[{""attributes"": [""2011-01""],
                                                            ""metadata"": {""digest"": ""4dac11f9c09f3ebc8842790cd5dec24a""}}]");
                    Assert.AreEqual(expectedResponse.ToString(), response.ToString());
                }
            }
        }

        [Test]
        public void ServerShouldRespondToQueryEntityVersions()
        {
            var participant = new StubParticipant();
            using (var client = new JsonAmqpRpcClient("localhost", "QUEUE_NAME"))
            {
                using (var server = new JsonAmqpRpcServer("localhost", "QUEUE_NAME", new ParticipantHandler(participant)))
                {
                    server.Start();
                    var json = @"[
	                    {
		                    ""attributes"": {""lower"": ""2011-01-01T00:00:00.000Z"",
									         ""upper"": ""2011-12-31T23:59:59.999Z""},
		                    ""values"": null,
		                    ""category"": ""bizDate""
                        }
                    ]";
                    var response = client.Call("query_entity_versions", JArray.Parse(json), 5000);
                    var expectedResponse = JArray.Parse(@"[{
                        ""attributes"": [""abc"", ""def""],
                        ""metadata"": {""digest"": ""vsn1"", ""id"": ""id1"", ""lastUpdated"": ""0001-01-01T00:00:00.0000000Z""}
                    }]");
                    Assert.AreEqual(expectedResponse.ToString(), response.ToString());
                }
            }
        }

        [Test]
        public void ServerShouldRespondToInvoke()
        {
            var participant = new StubParticipant();
            using (var client = new JsonAmqpRpcClient("localhost", "QUEUE_NAME"))
            {
                using (var server = new JsonAmqpRpcServer("localhost", "QUEUE_NAME", new ParticipantHandler(participant)))
                {
                    server.Start();
                    var response = client.Call("invoke", JObject.Parse(@"{""actionId"": ""someAction"", ""entityId"": ""f00""}"), 5000);
                    Assert.AreEqual(JObject.Parse(@"{""result"": ""RESULT"", ""output"": ""OUTPUT""}").ToString(), response.ToString());
                }
            }
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfStartCalledMultipleTimes()
        {
            var participant = new StubParticipant();
            using (var server = new JsonAmqpRpcServer("localhost", "QUEUE_NAME", new ParticipantHandler(participant)))
            {
                server.Start();
                server.Start();
            }
        }

        private class StubParticipant : IParticipant
        {
            public QueryAggregateDigestsResponse QueryAggregateDigests(QueryAggregateDigestsRequest request)
            {
                return new QueryAggregateDigestsResponse(new List<AggregateDigest> {
                    new AggregateDigest(new List<string> { "2011-01" }, "4dac11f9c09f3ebc8842790cd5dec24a") });
            }

            public QueryEntityVersionsResponse QueryEntityVersions(QueryEntityVersionsRequest request)
            {
                return new QueryEntityVersionsResponse(new List<EntityVersion>
                    {
                          new EntityVersion("id1", new List<string> {"abc", "def"}, new DateTime(), "vsn1")                                     
                    });
            }

            public InvocationResult Invoke(ActionInvocation request)
            {
                return new InvocationResult("RESULT", "OUTPUT");
            }
        }

    }
}
