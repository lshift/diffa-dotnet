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
    public class IntegerCategoryFunction : BaseCategoryFunction
    {
        private readonly int _denominator;

        public IntegerCategoryFunction(string attrName, int denominator) : base(attrName)
        {
            _denominator = denominator;
        }

        public override string Name
        {
            get { return _denominator + "s"; }
        }

        public override string OwningPartition(string value)
        {
            try
            {
                return (_denominator*(Int32.Parse(value)/_denominator)).ToString();
            }
            catch (FormatException)
            {
                throw new InvalidAttributeValueException();
            }
        }

        public int Denominator { get { return _denominator; } }
    }
}
