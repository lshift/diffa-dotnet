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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Net.LShift.Diffa.Participants;

namespace Net.LShift.Diffa.Messaging.Amqp
{
    /// <summary>
    /// A handler is able to handle JSON requests and produce a JSON response
    /// </summary>
    public interface IJsonRpcHandler
    {
        /// <summary>
        /// Handle a JSON request and produce a JSON response
        /// </summary>
        JsonTransportResponse HandleRequest(JsonTransportRequest request);
    }

    /// <summary>
    /// Handles JSON requests, routing them to the correct endpoint on a Participant and producing a JSON response
    /// </summary>
    public class ParticipantHandler : IJsonRpcHandler
    {
        private readonly IParticipant _participant;

        public ParticipantHandler(IParticipant participant)
        {
            _participant = participant;
        }

        public JsonTransportResponse HandleRequest(JsonTransportRequest request)
        {
            switch (request.Endpoint)
            {
                case "query_aggregate_digests":
                    return HandleQueryAggregateDigestsRequest(request);
                case "query_entity_versions":
                    return HandleQueryEntityVersionsRequest(request);
                case "invoke":
                    return HandleActionInvocation(request);
                case "retrieve_content":
                    return HandleRetrieveContent(request);
                default:
                    return JsonTransportResponse.NotFound("Endpoint '"+request.Endpoint+@"' not implemented");
            }
        }

        private JsonTransportResponse HandleQueryAggregateDigestsRequest(JsonTransportRequest request)
        {
            var requestParams = QueryAggregateDigestsRequest.FromJObject(request.Body);
            var response = _participant.QueryAggregateDigests(requestParams);
            return new JsonTransportResponse(200, response.ToJArray());
        }

        private JsonTransportResponse HandleQueryEntityVersionsRequest(JsonTransportRequest request)
        {
            var requestParams = QueryEntityVersionsRequest.FromJArray((JArray) request.Body);
            var response = _participant.QueryEntityVersions(requestParams);
            return new JsonTransportResponse(200, response.ToJArray());
        }

        private JsonTransportResponse HandleActionInvocation(JsonTransportRequest request)
        {
            // TODO un-fudge this DeserializeObject(JContainer.ToString()) nonsense
            var requestParams = JsonConvert.DeserializeObject<ActionInvocation>(request.Body.ToString());
            var response = _participant.Invoke(requestParams);
            return new JsonTransportResponse(200, JObject.FromObject(response));
        }

        private JsonTransportResponse HandleRetrieveContent(JsonTransportRequest request)
        {
            var requestParams = JsonConvert.DeserializeObject<EntityContentRequest>(request.Body.ToString());
            var response = _participant.RetrieveEntityContent(requestParams);
            return new JsonTransportResponse(200, JObject.FromObject(response));
        }
    }
}
