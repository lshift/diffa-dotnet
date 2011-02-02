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
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Net.LShift.Diffa.Participants
{
    public class DigestBuilder
    {
        private readonly Dictionary<string, ICategoryFunction> _functions;
        private readonly Dictionary<string, Bucket> _digestBuckets = new Dictionary<string, Bucket>();

        public DigestBuilder(IDictionary<string, string> functions)
        {
            _functions = functions.Keys.ToDictionary(key => key, key => NamesToTypes[functions[key]]);
        }

        public void Add(string id, IDictionary<string, string> attributes, DateTime lastUpdated, string version)
        {
            // Find the bucket name for each of attributes
            var partitions = attributes.Keys.ToDictionary(key => key, key => _functions[key].OwningPartition(attributes[key]));

            // Join names together to form label
            var keys = partitions.Keys.ToArray();
            Array.Sort(keys);
            var label = String.Join("_", from key in keys select partitions[key]);

            if(!_digestBuckets.ContainsKey(label))
            {
                _digestBuckets[label] = new Bucket(label, partitions, lastUpdated);
            }
            _digestBuckets[label].Add(version);
        }

        public IList<AggregateDigest> GetDigests()
        {
            var keys = _digestBuckets.Keys.ToArray();
            Array.Sort(keys);
            return new List<AggregateDigest>(from key in keys select _digestBuckets[key].ToDigest());
        }

        private static readonly IDictionary<string, ICategoryFunction> NamesToTypes = new Dictionary<string, ICategoryFunction>
            {
                {"1000s", new IntegerCategoryFunction(1000)},
                {"100s", new IntegerCategoryFunction(100)},
                {"10s", new IntegerCategoryFunction(10)}
            };
    }

    class Bucket
    {
        private string _digest;
        private readonly StringBuilder _builder = new StringBuilder();

        public string Name { get; private set; }
        public IDictionary<string, string> Attributes { get; private set; }
        public DateTime LastUpdated { get; private set; }

        public Bucket(string name, IDictionary<string, string> attributes, DateTime lastUpdated)
        {
            Name = name;
            Attributes = attributes;
            LastUpdated = lastUpdated;
        }

        public void Add(string version)
        {
            if (_digest != null)
            {
                throw new InvalidOperationException();
            }
            _builder.Append(version);
        }

        public AggregateDigest ToDigest()
        {
            if (_digest == null)
            {
                using (var md5 = MD5.Create())
                {
                    var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(_builder.ToString()));
                    _digest = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }
            }
            var keys = Attributes.Keys.ToArray();
            Array.Sort(keys);
            var attributes = keys.Select(key => Attributes[key]).ToList();
            return new AggregateDigest(attributes, LastUpdated, _digest);
        }
    }
}
