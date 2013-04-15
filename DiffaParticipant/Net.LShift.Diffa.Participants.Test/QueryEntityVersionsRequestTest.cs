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
    class QueryEntityVersionsRequestTest
    {
        [Test]
        public void ShouldCreateRequestFromJArray()
        {
            var jsonString = @"[
	            {
		            ""attributes"": {""lower"": ""2011-01-01T00:00:00.000Z"",
									 ""upper"": ""2011-12-31T23:59:59.999Z""},
		            ""values"": null,
		            ""category"": ""bizDate""
                }
            ]";
            var wireConstraints = JArray.Parse(jsonString);
            var request = QueryEntityVersionsRequest.FromJArray(wireConstraints);

            Assert.AreEqual("bizDate", request.Constraints[0].Category);
            Assert.AreEqual(null, request.Constraints[0].Values);
            Assert.AreEqual(new DateTime(2011, 1, 1), DateTime.Parse(request.Constraints[0].Attributes["lower"]).ToUniversalTime());
            Assert.AreEqual(new DateTime(2011, 12, 31, 23, 59, 59, 999),
                            DateTime.Parse(request.Constraints[0].Attributes["upper"]).ToUniversalTime());
        }

        [Test]
        public void ShouldSerializeResponseToJArray()
        {
            var jsonString = @"[
	            {
		            ""attributes"": [""2011-02-06T15:37:54.812Z""],
	 	            ""metadata"": {""digest"": ""vsn_abcdef"",
							       ""id"": ""abc"",
						           ""lastUpdated"": ""2011-02-06T15:37:00.0000000Z""}
              }
            ]";
            var expected = JArray.Parse(jsonString);
            var queryEntityVersionsResponse = new QueryEntityVersionsResponse(
                new List<EntityVersion> {
                    new EntityVersion("abc", new List<string> {"2011-02-06T15:37:54.812Z"}, new DateTime(2011, 2, 6, 15, 37, 0, 0, DateTimeKind.Utc), "vsn_abcdef")
                });

            Assert.AreEqual(expected.ToString(), queryEntityVersionsResponse.ToJArray().ToString());
        }
    }
}
