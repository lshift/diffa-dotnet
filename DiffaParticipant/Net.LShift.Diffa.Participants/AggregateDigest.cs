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

using Newtonsoft.Json.Linq;

namespace Net.LShift.Diffa.Participants
{
    public class AggregateDigest : IDigest
    {
        const string AttributesKey = "attributes";
        const string MetadataKey = "metadata";
        const string LastUpdatedKey = "lastUpdated";
        const string DigestKey = "digest";

        public AggregateDigest(IList<string> attributes, DateTime lastUpdated, string digest)
        {
            Attributes = attributes;
            LastUpdated = lastUpdated;
            Digest = digest;
        }

        public bool Equals(AggregateDigest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Attributes, Attributes) && other.LastUpdated.Equals(LastUpdated) && Equals(other.Digest, Digest);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (AggregateDigest)) return false;
            return Equals((AggregateDigest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Attributes != null ? Attributes.GetHashCode() : 0);
                result = (result*397) ^ LastUpdated.GetHashCode();
                result = (result*397) ^ (Digest != null ? Digest.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(AggregateDigest left, AggregateDigest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AggregateDigest left, AggregateDigest right)
        {
            return !Equals(left, right);
        }

        public IList<string> Attributes { get; private set; }

        public DateTime LastUpdated { get; private set; }

        public string Digest { get; private set; }

        public JObject ToJObject()
        {
            return JObject.FromObject(new Dictionary<string, object>
                {
                    {AttributesKey, Attributes},
                    {MetadataKey, new Dictionary<string, object>
                        {
                            {LastUpdatedKey, LastUpdated.ToUniversalTime().ToString("o")},
                            {DigestKey, Digest}
                        }}
                });
        }

        public override string ToString()
        {
            return "AggregateDigest(Attributes=[" + String.Join(", ", Attributes) + "], LastUpdated="
                + LastUpdated.ToUniversalTime().ToString("o") + ", Digest=" + Digest + ")";
        }
    }
}
