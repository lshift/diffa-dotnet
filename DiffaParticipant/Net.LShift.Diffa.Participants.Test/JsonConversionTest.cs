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

using Newtonsoft.Json.Linq;

namespace Net.LShift.Diffa.Participants.Test
{
    [TestFixture]
    public class JsonConversionTest
    {
        [Test]
        public void ShouldCreateRequestFromJObject()
        {
            var jsonString = @"{
                ""constraints"": [
                {
                    ""attributes"": {""lower"": ""2011-01-01T00:00:00.000Z"",
                                     ""upper"": ""2011-12-31T23:59:59.999Z""},
                    ""values"": null,
                    ""dataType"": ""bizDate""
                }
                ],
                ""buckets"": {""bizDate"": ""yearly""}
            }";
            var jObject = JObject.Parse(jsonString);
            var request = QueryAggregateDigestsRequest.FromJObject(jObject);

            Assert.AreEqual("yearly", request.Buckets["bizDate"]);
            Assert.AreEqual("bizDate", request.Constraints[0].DataType);
            Assert.AreEqual(null, request.Constraints[0].Values);
            Assert.AreEqual(new DateTime(2011, 1, 1), DateTime.Parse(request.Constraints[0].Attributes["lower"]));
            Assert.AreEqual(new DateTime(2011, 12, 31, 23, 59, 59, 999),
                            DateTime.Parse(request.Constraints[0].Attributes["upper"]));
        }

        [Test]
        public void ShouldSerializeResponseToJArray()
        {
            var jsonString =
                @"[{
                ""attributes"": [""2011-01""],
                ""metadata"": {""lastUpdated"": ""2011-01-31T16:22:23.7240000Z"",
                               ""digest"": ""4dac11f9c09f3ebc8842790cd5dec24a""}
              }]";
            var expected = JArray.Parse(jsonString);
            var queryAggregateDigestsResponse = new QueryAggregateDigestsResponse(
                new List<AggregateDigest>() {
                    new AggregateDigest(new List<string> {"2011-01"}, new DateTime(2011, 01, 31, 16, 22, 23, 724),
                        "4dac11f9c09f3ebc8842790cd5dec24a")});

            Assert.AreEqual(expected.ToString(), queryAggregateDigestsResponse.ToJArray().ToString());
        }
    }
}
