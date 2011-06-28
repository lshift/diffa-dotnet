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

namespace Net.LShift.Diffa.Participants
{
    public interface ICategoryFunction
    {
        /// <summary>
        /// The name of this function.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The name of the attribute that this function is being applied to. 
        /// </summary>
        string AttributeName { get;  }

        /// <summary>
        /// Given a value from the value domain, returns the name of the partition to which it belongs.
        /// </summary>
        /// <param name="value">Value from the value domain encoded as a string</param>
        /// <returns>The name of the partition to which the value belongs</returns>
        /// <exception cref="InvalidAttributeValueException">value is not valid for this category function</exception>
        string OwningPartition(string value);
    }

    public class InvalidAttributeValueException : ArgumentException {}
}
