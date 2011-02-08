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
using System.Diagnostics;
using Net.LShift.Diffa.Participants;
using Newtonsoft.Json.Linq;

namespace Net.LShift.Diffa.Messaging.Amqp
{
    public interface IJsonRpcHandler
    {
        JsonTransportResponse HandleRequest(JsonTransportRequest request);
    }

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
                default:
                    return new JsonTransportResponse(404, JArray.Parse(@"[{""error"": ""Endpoint not implemented""}]"));
            }
        }

        private JsonTransportResponse HandleQueryAggregateDigestsRequest(JsonTransportRequest request)
        {
            var requestParams = QueryAggregateDigestsRequest.FromJObject(request.Body);
            var response = _participant.QueryAggregateDigests(requestParams);
            Debug.Assert(response != null);
            return new JsonTransportResponse(200, response.ToJArray());
        }

        private JsonTransportResponse HandleQueryEntityVersionsRequest(JsonTransportRequest request)
        {
            var requestParams = QueryEntityVersionsRequest.FromJArray((JArray) request.Body);
            var response = _participant.QueryEntityVersions(requestParams);
            Debug.Assert(response != null);
            return new JsonTransportResponse(200, response.ToJArray());
        }
    }
}
