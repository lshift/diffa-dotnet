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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Net.LShift.Diffa.Participants
{
    public class QueryAggregateDigestsRequest
    {
        public IList<IDictionary<string, object>> Constraints { get; private set; }
        public IDictionary<string, string> Buckets { get; private set; }

        public QueryAggregateDigestsRequest(IList<IDictionary<string, object>> constraints, IDictionary<string, string> buckets)
        {
            Constraints = constraints;
            Buckets = buckets;
        }

        public static QueryAggregateDigestsRequest FromJObject(JObject jObject)
        {
            var request = JsonConvert.DeserializeObject<QueryAggregateDigestsRequest>(jObject.ToString());
            if (request.Constraints == null || request.Buckets == null)
            {
                throw new ArgumentNullException();
            }
            return request;
        }
    }

    public class QueryAggregateDigestsResponse
    {
        public int Status { get; private set; }
        public string Digest { get; private set; }

        public QueryAggregateDigestsResponse(int status, string digest)
        {
            Status = status;
            Digest = digest;
        }

        public JObject ToJObject()
        {
            return new JObject(); // TODO
        }
    }

}

