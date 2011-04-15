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
using NLog.Config;
using NLog.Win32.Targets;
using NUnit.Framework;
using Rhino.Mocks;

using NLog;

using Newtonsoft.Json.Linq;

using Net.LShift.Diffa.Participants;

namespace Net.LShift.Diffa.Messaging.Amqp.Test
{
    [TestFixture]
    public class JsonAmqpRpcServerTest
    {

        private readonly MockRepository _mockery = new MockRepository();
        private readonly Logger _log = CreateLogging();

        private IParticipant _participant;
        private JsonAmqpRpcServer _server;
        private JsonAmqpRpcClient _client;

        [SetUp]
        public void SetUp()
        {
            var participant = new StubParticipant();
            _server = new JsonAmqpRpcServer("localhost", "QUEUE_NAME", new ParticipantHandler(participant));
            _server.Start();
            _client = new JsonAmqpRpcClient("localhost", "QUEUE_NAME");
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _server.Dispose();
        }

        [Test]
        public void ServerShouldRespondToQueryAggregateDigests()
        {
            var response = _client.Call("query_aggregate_digests", JObject.Parse(@"{""constraints"": [], ""buckets"": {}}"));
            var expectedResponse = JArray.Parse(@"[{""attributes"": [""2011-01""],
                                                    ""metadata"": {""digest"": ""4dac11f9c09f3ebc8842790cd5dec24a""}}]");
            Assert.AreEqual(expectedResponse.ToString(), response.ToString());
        }

        [Test]
        public void ServerShouldRespondToQueryEntityVersions()
        {
            var json = @"[
	            {
		            ""attributes"": {""lower"": ""2011-01-01T00:00:00.000Z"",
									    ""upper"": ""2011-12-31T23:59:59.999Z""},
		            ""values"": null,
		            ""category"": ""bizDate""
                }
            ]";
            var response = _client.Call("query_entity_versions", JArray.Parse(json), 5000);
            var expectedResponse = JArray.Parse(@"[{
                ""attributes"": [""abc"", ""def""],
                ""metadata"": {""digest"": ""vsn1"", ""id"": ""id1"", ""lastUpdated"": ""0001-01-01T00:00:00.0000000Z""}
            }]");
            Assert.AreEqual(expectedResponse.ToString(), response.ToString());
        }

        [Test]
        public void ServerShouldRespondToInvoke()
        {
            var response = _client.Call("invoke", JObject.Parse(@"{""actionId"": ""someAction"", ""entityId"": ""f00""}"), 5000);
            Assert.AreEqual(JObject.Parse(@"{""result"": ""RESULT"", ""output"": ""OUTPUT""}").ToString(), response.ToString());
        }

        [Test]
        public void ServerShouldRespondToRetrieveContent()
        {
            var response = _client.Call("retrieve_content", JObject.Parse(@"{""id"": ""123""}"), 5000);
            Assert.AreEqual(JObject.Parse(@"{""content"": ""CONTENT""}").ToString(), response.ToString());
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfStartCalledMultipleTimes()
        {
            _server.Start();
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

            public EntityContentResponse RetrieveEntityContent(EntityContentRequest request)
            {
                return new EntityContentResponse("CONTENT");
            }
        }

        private static Logger CreateLogging()
        {
            var config = new LoggingConfiguration();
            var target = new ColoredConsoleTarget() {Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}"};
            config.AddTarget("console", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;
            return LogManager.GetCurrentClassLogger();
        }
    }
}
