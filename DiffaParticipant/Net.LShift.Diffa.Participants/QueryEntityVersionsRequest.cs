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

    public class QueryEntityVersionsRequest
    {
        public IList<WireConstraint> Constraints { get; private set; }

        public static QueryEntityVersionsRequest FromJArray(JArray jArray)
        {
            var constraints = jArray.Children().Select(child => JsonConvert.DeserializeObject<WireConstraint>(child.ToString())).ToList();
            return new QueryEntityVersionsRequest {Constraints = constraints};
        }

        public IList<IQueryConstraint> GetQueryConstraints()
        {
            return Constraints.Select(constraint => constraint.ToQueryConstraint()).ToList();
        } 
    }

    public class QueryEntityVersionsResponse
    {
        public IList<EntityVersion> EntityVersions { get; private set; }

        public QueryEntityVersionsResponse(IList<EntityVersion> entityVersions)
        {
            EntityVersions = entityVersions;
        }

        public JArray ToJArray()
        {
            var jArray = new JArray();
            foreach (var version in EntityVersions)
            {
                jArray.Add(version.ToJObject());
            }
            return jArray;
        }
    }
}
