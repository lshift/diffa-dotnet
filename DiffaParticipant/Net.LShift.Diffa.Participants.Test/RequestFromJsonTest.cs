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

using System.Collections.Generic;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

namespace Net.LShift.Diffa.Participants.Test
{
    [TestFixture]
    public class RequestFromJsonTest
    {
        [Test]
        public void ShouldCreateRequestFromJson()
        {
            var jsonString = @"{
                ""constraints"": [
                    {""attributes"": {}, ""values"": [], ""dataType"": ""bizDate""}
                ],
                ""buckets"": {""bizDate"": ""yearly""}
            }";
            var jObject = JObject.Parse(jsonString);
            var request = QueryAggregateDigestsRequest.FromJObject(jObject);

            Assert.AreEqual("yearly", request.Buckets["bizDate"]);
            Assert.AreEqual("bizDate", request.Constraints[0]["dataType"]);
            Assert.AreEqual(new List<string>(), request.Constraints[0]["values"]);
            Assert.AreEqual(new Dictionary<string, string>(), request.Constraints[0]["attributes"]);
        }
    }
}
