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
using System.Linq;
using System.Text;
using Net.LShift.Diffa.Participants.Utils;

namespace Net.LShift.Diffa.Participants
{
    public class DigestBuilder
    {
        private readonly Dictionary<string, ICategoryFunction> _functions;
        private readonly Dictionary<BucketKey, Bucket> _digestBuckets = new Dictionary<BucketKey, Bucket>();

        public DigestBuilder(IEnumerable<ICategoryFunction> functions) {
          _functions = functions.ToDictionary(f => f.AttributeName, f => f);
        }

        public DigestBuilder(IDictionary<string, string> functions)
        {
            _functions = functions.Keys.ToDictionary(key => key, key => CategoryFunctionRegistry.GetByName(functions[key]));
        }

        public void Add(string id, IDictionary<string, string> attributes, string version)
        {
          // Find the bucket name for each of attributes
          var partitions = new Dictionary<string, string>(attributes);
          foreach (var kv in attributes.Where(kv => _functions.ContainsKey(kv.Key)))
          {
              var attributeValue = attributes[kv.Key];
              var categoryFunction = _functions[kv.Key];
              partitions[kv.Key] = categoryFunction.OwningPartition(attributeValue);
          }

          var key = new BucketKey(partitions);
          if(!_digestBuckets.ContainsKey(key))
          {
              var bucket = new Bucket(key, partitions);
              _digestBuckets[key] = bucket;
          }
          _digestBuckets[key].Add(version);
        }

        public IList<AggregateDigest> GetDigests()
        {
          var keys = _digestBuckets.Keys.ToArray();
          Array.Sort(keys, (k1, k2) => k1.Name.CompareTo(k2.Name));
          return new List<AggregateDigest>(from key in keys select _digestBuckets[key].ToDigest());
        }

        public IList<ScanResultEntry> GetScanResults() {
          var keys = _digestBuckets.Keys.ToArray();
          return new List<ScanResultEntry>(from key in keys select _digestBuckets[key].ToScanResultEntry());
        }

    }

    internal class CategoryFunctionRegistry
    {
        private static readonly IDictionary<string, ICategoryFunction> Registry = new Dictionary<string, ICategoryFunction>
            {
                {"1000s", new IntegerCategoryFunction(null, 1000)},
                {"100s", new IntegerCategoryFunction(null, 100)},
                {"10s", new IntegerCategoryFunction(null, 10)},
                {"daily", new DailyCategoryFunction(null)},
                {"monthly", new MonthlyCategoryFunction(null)},
                {"yearly", new YearlyCategoryFunction(null)},
                {"by name", new ByNameCategoryFunction(null)}
            };

        internal static ICategoryFunction GetByName(string name)
        {
            return Registry[name];
        }
    }

    class Bucket {
      private string _digest;
      private readonly StringBuilder _builder = new StringBuilder();

      public BucketKey Key { get; private set; }
      public IDictionary<string, string> Attributes { get; private set; }

      public Bucket(BucketKey key, IDictionary<string, string> attributes)
      {
        Key = key;
        Attributes = attributes;
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
            _digest = DigestUtils.Md5Hex(_builder.ToString());
        }
        var keys = Attributes.Keys.ToArray();
        Array.Sort(keys);
        var attributes = keys.Select(key => Attributes[key]).ToList();
        return new AggregateDigest(attributes, _digest);
      }

      public ScanResultEntry ToScanResultEntry() {
        if (_digest == null) {
          _digest = DigestUtils.Md5Hex(_builder.ToString());
        }

        return ScanResultEntry.ForAggregate(Attributes, _digest);
      }

      public override string ToString()
      {
          return "Bucket(Name=" + Key.Name + ", Attributes={" + String.Join(",", Attributes) + "})";
      }
    }

  class BucketKey {
    private readonly IDictionary<String, String> _attributes;
    private readonly string _name;

    public string Name { get { return _name; } }

    public BucketKey(IDictionary<String, String> attributes) {
      _attributes = attributes;

      var attrKeys = attributes.Keys.ToArray();
      Array.Sort(attrKeys);
      _name = String.Join("_", from attrKey in attrKeys select attributes[attrKey]);
    }

    public bool Equals(BucketKey other) {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      if (_attributes.Count != other._attributes.Count) return false;
      if (other._attributes.Intersect(_attributes).Count() != _attributes.Count()) return false;

      return true;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != typeof (BucketKey)) return false;
      return Equals((BucketKey) obj);
    }

    public override int GetHashCode() {
      return _name.GetHashCode();
    }
  }
}
