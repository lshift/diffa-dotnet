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
    /// <summary>
    /// A digest of aggregated versions
    /// </summary>
    public class AggregateDigest : IDigest
    {
        const string AttributesKey = "attributes";
        const string MetadataKey = "metadata";
        const string DigestKey = "digest";

        public AggregateDigest(IList<string> attributes, string digest)
        {
            Attributes = attributes;
            Digest = digest;
        }

        public IList<string> Attributes { get; private set; }

        public string Digest { get; private set; }

        public JObject ToJObject()
        {
            return JObject.FromObject(new Dictionary<string, object>
                {
                    {AttributesKey, Attributes},
                    {MetadataKey, new Dictionary<string, object>
                        {
                            {DigestKey, Digest}
                        }}
                });
        }

        public override string ToString()
        {
            return "AggregateDigest(Attributes=[" + String.Join(", ", Attributes) + "], Digest=" + Digest + ")";
        }
    }
}
