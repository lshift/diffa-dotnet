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
    /// The wire format for invoking an action on a particular entity
    /// </summary>
    public class ActionInvocation
    {
        public string ActionId { get; private set; }
        public string EntityId { get; private set; }

        public ActionInvocation(string actionId, string entityId)
        {
            ActionId = actionId;
            EntityId = entityId;
        }
    }

    /// <summary>
    /// Encapsulates the result of invoking an action against a participant
    /// </summary>
    public class InvocationResult
    {
        public string result { get; private set; }
        public string output { get; private set; }

        public InvocationResult(string result, string output)
        {
            this.result = result;
            this.output = output;
        }
    }
}
