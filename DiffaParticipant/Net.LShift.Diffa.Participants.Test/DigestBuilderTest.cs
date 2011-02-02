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

namespace Net.LShift.Diffa.Participants.Test
{
    [TestFixture]
    class DigestBuilderTest
    {
        [Test]
        public void TestAddAndDigest()
        {
            var buckets = new Dictionary<string, string> {{"someInt", "1000s"}};
            var builder = new DigestBuilder(buckets);
            
            var lastUpdated = DateTime.Parse("2011-02-02T14:23:44.426Z");

            builder.Add("id1", new Dictionary<string, string> { { "someInt", "1234" } }, lastUpdated, "vsn1");
            builder.Add("id2", new Dictionary<string, string> { { "someInt", "2345" } }, lastUpdated, "vsn2");

            var expected = new List<AggregateDigest>
                {
                    new AggregateDigest(new List<string> {"1000"}, lastUpdated, "23fa1dfd4d2a8240fa54ea7a5b4ba62e"),
                    new AggregateDigest(new List<string> {"2000"}, lastUpdated, "8b5452fe0b023c8644913c1ebaccf310")
                };
            var aggregateDigests = builder.GetDigests();

            // TODO fudged because Assert.AreEqual did not seem to work as expected on the actual AggregateDigest objects
            Assert.AreEqual(expected[0].LastUpdated, aggregateDigests[0].LastUpdated);
            Assert.AreEqual(expected[0].Attributes, aggregateDigests[0].Attributes);
            Assert.AreEqual(expected[0].Digest, aggregateDigests[0].Digest);
            Assert.AreEqual(expected[1].LastUpdated, aggregateDigests[1].LastUpdated);
            Assert.AreEqual(expected[1].Attributes, aggregateDigests[1].Attributes);
            Assert.AreEqual(expected[1].Digest, aggregateDigests[1].Digest);
        }
    }
}
