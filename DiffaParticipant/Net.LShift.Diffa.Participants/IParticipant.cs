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

namespace Net.LShift.Diffa.Participants
{
    /// <summary>
    /// Interface that a participant should implement in order to be queried by the RPC server
    /// </summary>
    public interface IParticipant
    {
        /// <summary>
        /// Produce an aggregate of digests over numerous entities, partitioned (bucketed) according to constraints
        /// </summary>
        QueryAggregateDigestsResponse QueryAggregateDigests(QueryAggregateDigestsRequest request);

        /// <summary>
        /// Produce a sequence of digests for individual entities
        /// </summary>
        QueryEntityVersionsResponse QueryEntityVersions(QueryEntityVersionsRequest request);

        /// <summary>
        /// Invoke an arbitrary named action. May produce an error response if the named action is not implemented
        /// </summary>
        InvocationResult Invoke(ActionInvocation request);

        /// <summary>
        /// Produce a serialized version of a particular entity specified by its ID
        /// </summary>
        EntityContentResponse RetrieveEntityContent(EntityContentRequest request);
    }
}
