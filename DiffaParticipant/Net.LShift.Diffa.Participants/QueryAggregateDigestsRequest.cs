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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Net.LShift.Diffa.Participants
{
    public class QueryAggregateDigestsRequest
    {
        public IList<WireConstraint> Constraints { get; private set; }
        public IDictionary<string, string> Buckets { get; private set; }

        public QueryAggregateDigestsRequest(IList<WireConstraint> constraints, IDictionary<string, string> buckets)
        {
            Constraints = constraints;
            Buckets = buckets;
        }

        public IList<IQueryConstraint> GetQueryConstraints()
        {
            return Constraints.Select(constraint => constraint.ToQueryConstraint()).ToList();
        } 

        public static QueryAggregateDigestsRequest FromJObject(JContainer jObject)
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
        public IList<AggregateDigest> Digests { get; private set; }

        public QueryAggregateDigestsResponse(IList<AggregateDigest> digests)
        {
            Digests = digests;
        }

        public JArray ToJArray()
        {
            var jArray = new JArray();
            foreach (var digest in Digests)
            {
                jArray.Add(digest.ToJObject());
            }
            return jArray;
        }
    }
    
}

