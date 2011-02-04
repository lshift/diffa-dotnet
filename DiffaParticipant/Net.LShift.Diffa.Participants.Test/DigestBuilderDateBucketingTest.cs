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
    class DigestBuilderDateBucketingTest
    {
        [Test]
        public void ShouldBucketByDay()
        {
            var buckets = new Dictionary<string, string> { { "bizDate", "daily" } };
            var builder = new DigestBuilder(buckets);

            var lastUpdated = DateTime.Parse("2011-02-02T14:23:44.426Z");

            builder.Add("id1", new Dictionary<string, string> { { "bizDate", "2010-08-16" } }, "vsn1");
            builder.Add("id2", new Dictionary<string, string> { { "bizDate", "2010-08-14" } }, "vsn2");
            builder.Add("id3", new Dictionary<string, string> { { "bizDate", "2010-08-16" } }, "vsn3");

            var expected = new List<AggregateDigest>
                {
                    new AggregateDigest(new List<string> {"2010-08-14"}, DigestUtils.Md5Hex("vsn2")),
                    new AggregateDigest(new List<string> {"2010-08-16"}, DigestUtils.Md5Hex("vsn1"+"vsn3"))
                };
            var aggregateDigests = builder.GetDigests();

            // TODO fudged because Assert.AreEqual did not seem to work as expected on the actual AggregateDigest objects
            Assert.AreEqual(expected[0].Attributes, aggregateDigests[0].Attributes);
            Assert.AreEqual(expected[0].Digest, aggregateDigests[0].Digest);
            Assert.AreEqual(expected[1].Attributes, aggregateDigests[1].Attributes);
            Assert.AreEqual(expected[1].Digest, aggregateDigests[1].Digest);
        }

        [Test]
        public void ShouldBucketByMonth()
        {
            var buckets = new Dictionary<string, string> { { "bizDate", "monthly" } };
            var builder = new DigestBuilder(buckets);

            var lastUpdated = DateTime.Parse("2011-02-02T14:23:44.426Z");

            builder.Add("id1", new Dictionary<string, string> { { "bizDate", "2010-08-16" } }, "vsn1");
            builder.Add("id2", new Dictionary<string, string> { { "bizDate", "2010-07-14" } }, "vsn2");
            builder.Add("id3", new Dictionary<string, string> { { "bizDate", "2010-08-09" } }, "vsn3");

            var expected = new List<AggregateDigest>
                {
                    new AggregateDigest(new List<string> {"2010-07"}, DigestUtils.Md5Hex("vsn2")),
                    new AggregateDigest(new List<string> {"2010-08"}, DigestUtils.Md5Hex("vsn1"+"vsn3"))
                };
            var aggregateDigests = builder.GetDigests();

            // TODO fudged because Assert.AreEqual did not seem to work as expected on the actual AggregateDigest objects
            Assert.AreEqual(expected[0].Attributes, aggregateDigests[0].Attributes);
            Assert.AreEqual(expected[0].Digest, aggregateDigests[0].Digest);
            Assert.AreEqual(expected[1].Attributes, aggregateDigests[1].Attributes);
            Assert.AreEqual(expected[1].Digest, aggregateDigests[1].Digest);
        }

        [Test]
        public void ShouldBucketByYear()
        {
            var buckets = new Dictionary<string, string> { { "bizDate", "yearly" } };
            var builder = new DigestBuilder(buckets);

            var lastUpdated = DateTime.Parse("2011-02-02T14:23:44.426Z");

            builder.Add("id1", new Dictionary<string, string> { { "bizDate", "2010-08-16" } }, "vsn1");
            builder.Add("id2", new Dictionary<string, string> { { "bizDate", "2009-07-14" } }, "vsn2");
            builder.Add("id3", new Dictionary<string, string> { { "bizDate", "2010-08-09" } }, "vsn3");

            var expected = new List<AggregateDigest>
                {
                    new AggregateDigest(new List<string> {"2009"}, DigestUtils.Md5Hex("vsn2")),
                    new AggregateDigest(new List<string> {"2010"}, DigestUtils.Md5Hex("vsn1"+"vsn3"))
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
