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

namespace Net.LShift.Diffa.Participants
{
    public interface IQueryConstraint
    {
        string DataType { get; }
    }

    /// <summary>
    /// Represents a set of values with which to constrain
    /// </summary>
    public class ListQueryConstraint : IQueryConstraint
    {
        public string DataType { get; private set; }
        public IList<string> Values { get; private set; }

        public ListQueryConstraint(string dataType, IList<string> values)
        {
            DataType = dataType;
            Values = values;
        }
    }

    /// <summary>
    /// Represents a range of values as a sequence containing upper and lower bounds
    /// </summary>
    public class RangeQueryConstraint : IQueryConstraint
    {
        public string DataType { get; private set; }
        public string LowerBound { get; private set; }
        public string UpperBound { get; private set; }

        public RangeQueryConstraint(string dataType, string lowerBound, string upperBound)
        {
            DataType = dataType;
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }
    }

    /// <summary>
    /// Represents a range with no bounds
    /// </summary>
    public class UnboundedRangeQueryConstraint : IQueryConstraint
    {
        public string DataType { get; private set; }

        public UnboundedRangeQueryConstraint(string dataType)
        {
            DataType = dataType;
        }
    }

    /// <summary>
    /// Form of a Query Constraint which can easily be read from a wire
    /// </summary>
    public class WireConstraint
    {
        public string DataType { get; private set; }
        public IDictionary<string, string> Attributes { get; private set; }
        public IList<string> Values { get; private set; }

        public WireConstraint(string dataType, IDictionary<string, string> attributes, IList<string> values)
        {
            DataType = dataType;
            Attributes = attributes;
            Values = values;
        }

        public IQueryConstraint ToQueryConstraint()
        {
            Validate();
            if (Values != null)
            {
                return new ListQueryConstraint(DataType, Values);
            }
            var lower = Attributes[Lower];
            var upper = Attributes[Upper];
            if (lower != null && upper != null)
            {
                return new RangeQueryConstraint(DataType, lower, upper);
            }
            return new UnboundedRangeQueryConstraint(DataType);
        }

        private void Validate()
        {
            // TODO provide messages
            if (DataType == null || Attributes == null)
            {
                throw new InvalidWireConstraint();
            }
            if (Values != null)
            {
                if (Attributes.ContainsKey(Lower) && Attributes.ContainsKey(Upper))
                {
                    throw new InvalidWireConstraint();
                }
            }
            if ((! Attributes.ContainsKey(Lower)) && Attributes.ContainsKey(Upper))
            {
                throw new InvalidWireConstraint();
            }
        }

        private const string Lower = "lower";
        private const string Upper = "upper";
    }

    public class InvalidWireConstraint : Exception
    {
        // TODO constructor which accepts a message
    }
}
