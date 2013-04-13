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
using Newtonsoft.Json.Linq;

namespace Net.LShift.Diffa.Participants
{
  /// <summary>
  /// Describes an entry within a Scan Result.
  /// </summary>
  public class ScanResultEntry {
    public string ID { get; private set; }
    public IDictionary<string, string> Attributes { get; private set; }
    public DateTime? LastUpdated { get; private set; }
    public string Version { get; private set; }

    public static ScanResultEntry ForEntity(string id, IDictionary<string, string> attributes, DateTime? lastUpdated, string version) {
      return new ScanResultEntry(id, attributes, lastUpdated, version);
    }
    public static ScanResultEntry ForAggregate(IDictionary<string, string> attributes, string version) {
      return new ScanResultEntry(null, attributes, null, version);
    }

    public ScanResultEntry(string id, IDictionary<string, string> attributes, DateTime? lastUpdated, string version) {
      ID = id;
      Attributes = attributes;
      LastUpdated = lastUpdated;
      Version = version;
    }

    public JObject ToJObject() {
      var result = new JObject();
      if (ID != null) result.Add("id", ID);
      if (LastUpdated != null) result.Add("lastUpdated", LastUpdated.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
      if (Version != null) result.Add("version", Version);
      if (Attributes != null) result.Add("attributes", JObject.FromObject(Attributes));

      return result;
    }

    public override string ToString()
    {
        var attributes = String.Join(", ", Attributes);
        var asString = string.Format("ScanResultEntry(Id={0},Attributes=[{1}],Version={2},LastUpdated={3})", ID, attributes, Version, LastUpdated);
        return asString;
    }
  }
}
