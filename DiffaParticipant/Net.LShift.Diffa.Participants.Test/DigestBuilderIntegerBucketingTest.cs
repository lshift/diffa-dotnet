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
using Net.LShift.Diffa.Participants.Utils;
using NUnit.Framework;

namespace Net.LShift.Diffa.Participants.Test
{
    [TestFixture]
    class DigestBuilderIntegerBucketingTest
    {
        [Test]
        public void ShouldBucketByThousands()
        {
            var buckets = new Dictionary<string, string> {{"someInt", "1000s"}};
            var builder = new DigestBuilder(buckets);
            
            builder.Add("id1", new Dictionary<string, string> { { "someInt", "1234" } }, "vsn1");
            builder.Add("id2", new Dictionary<string, string> { { "someInt", "2345" } }, "vsn2");
            builder.Add("id3", new Dictionary<string, string> { { "someInt", "1235" } }, "vsn3" );

            var expected = new List<AggregateDigest>
                {
                    new AggregateDigest(new List<string> {"1000"}, DigestUtils.Md5Hex("vsn1"+"vsn3")),
                    new AggregateDigest(new List<string> {"2000"}, DigestUtils.Md5Hex("vsn2"))
                };
            var aggregateDigests = builder.GetDigests();

            // TODO fudged because Assert.AreEqual did not seem to work as expected on the actual AggregateDigest objects
            Assert.AreEqual(expected[0].Attributes, aggregateDigests[0].Attributes);
            Assert.AreEqual(expected[0].Digest, aggregateDigests[0].Digest);
            Assert.AreEqual(expected[1].Attributes, aggregateDigests[1].Attributes);
            Assert.AreEqual(expected[1].Digest, aggregateDigests[1].Digest);
        }

        [Test]
        public void ShouldBucketByHundreds()
        {
            var buckets = new Dictionary<string, string> { { "someInt", "100s" } };
            var builder = new DigestBuilder(buckets);

            builder.Add("id1", new Dictionary<string, string> { { "someInt", "123" } }, "vsn1");
            builder.Add("id2", new Dictionary<string, string> { { "someInt", "234" } }, "vsn2");
            builder.Add("id3", new Dictionary<string, string> { { "someInt", "125" } }, "vsn3");

            var expected = new List<AggregateDigest>
                {
                    new AggregateDigest(new List<string> {"100"}, DigestUtils.Md5Hex("vsn1"+"vsn3")),
                    new AggregateDigest(new List<string> {"200"}, DigestUtils.Md5Hex("vsn2"))
                };
            var aggregateDigests = builder.GetDigests();

            // TODO fudged because Assert.AreEqual did not seem to work as expected on the actual AggregateDigest objects
            Assert.AreEqual(expected[0].Attributes, aggregateDigests[0].Attributes);
            Assert.AreEqual(expected[0].Digest, aggregateDigests[0].Digest);
            Assert.AreEqual(expected[1].Attributes, aggregateDigests[1].Attributes);
            Assert.AreEqual(expected[1].Digest, aggregateDigests[1].Digest);
        }

    }
}
