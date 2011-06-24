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
    public abstract class DateCategoryFunction : BaseCategoryFunction
    {
        public abstract override string Name { get; }
        public abstract string Pattern { get; }

        protected DateCategoryFunction(string attributeName)
          : base(attributeName) {
        }

        public override string OwningPartition(string value)
        {
            try
            {
                var date = DateTime.Parse(value);
                return date.ToString(Pattern);
            }
            catch (FormatException)
            {
                throw new InvalidAttributeValueException();
            }
        }
    }

    public class DailyCategoryFunction : DateCategoryFunction
    {
        public DailyCategoryFunction(string attributeName) : base(attributeName) {
        }

        public override string Name
        {
            get { return "daily"; }
        }

        public override string Pattern
        {
            get { return "yyyy-MM-dd"; }
        }
    }

    public class MonthlyCategoryFunction : DateCategoryFunction
    {
        public MonthlyCategoryFunction(string attributeName) : base(attributeName) {
        }

        public override string Name
        {
            get { return "monthly"; }
        }

        public override string Pattern
        {
            get { return "yyyy-MM"; }
        }
    }

    public class YearlyCategoryFunction : DateCategoryFunction
    {
        public YearlyCategoryFunction(string attributeName) : base(attributeName) {
        }

        public override string Name
        {
            get { return "yearly"; }
        }

        public override string Pattern
        {
            get { return "yyyy"; }
        }
    }
}
