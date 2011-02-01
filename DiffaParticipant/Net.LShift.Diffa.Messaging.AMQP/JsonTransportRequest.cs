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

using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Net.LShift.Diffa.Messaging.Amqp
{
    public class JsonTransportRequest
    {
        public string Endpoint { get; private set; }
        public JObject Body { get; private set; }

        public JsonTransportRequest(string endpoint, JObject body)
        {
            Endpoint = endpoint;
            Body = body;
            Debug.Print("endpoint: "+endpoint+", body: "+body);
        }
    }
}
