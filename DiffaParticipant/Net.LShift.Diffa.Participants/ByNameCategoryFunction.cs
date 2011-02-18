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
    /// This function partitions based on a set of attribute names which individually form single level buckets
    /// </summary>
    class ByNameCategoryFunction : ICategoryFunction
    {
        public string Name
        {
            get { return "by name"; }
        }

        public string OwningPartition(string value)
        {
            return value;
        }
    }
}
