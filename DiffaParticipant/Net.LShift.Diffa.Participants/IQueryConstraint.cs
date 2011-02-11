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
        string Category { get; }
    }

    /// <summary>
    /// Represents a range of values as a sequence containing upper and lower bounds
    /// </summary>
    public class RangeQueryConstraint : IQueryConstraint
    {
        public string Category { get; private set; }
        public string LowerBound { get; private set; }
        public string UpperBound { get; private set; }

        public RangeQueryConstraint(string dataType, string lowerBound, string upperBound)
        {
            Category = dataType;
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public override string ToString()
        {
            return "RangeQueryConstraint(DataType="+Category+", LowerBound="+LowerBound+", UpperBound="+UpperBound+")";
        }
    }

    /// <summary>
    /// Represents a (not necessarily contiguous) set of values with which to constrain
    /// </summary>
    public class SetQueryConstraint : IQueryConstraint
    {
        public string Category { get; private set; }
        public ISet<string> Values { get; private set; }

        public SetQueryConstraint(string dataType, ISet<string> values)
        {
            Category = dataType;
            Values = values;
        }

        public override string ToString()
        {
            return "SetQueryConstraint(DataType=" + Category + ", Values=[" +
                   String.Join(", ", new List<string>(Values)) + "])";
        }
    }

    /// <summary>
    /// Represents a range with no bounds
    /// </summary>
    public class UnboundedRangeQueryConstraint : IQueryConstraint
    {
        public string Category { get; private set; }

        public UnboundedRangeQueryConstraint(string dataType)
        {
            Category = dataType;
        }
    }

    /// <summary>
    /// Form of a Query Constraint which can easily be read from a wire
    /// </summary>
    public class WireConstraint
    {
        public string Category { get; private set; }
        public IDictionary<string, string> Attributes { get; private set; }
        public IList<string> Values { get; private set; }

        public WireConstraint(string category, IDictionary<string, string> attributes, IList<string> values)
        {
            Category = category;
            Attributes = attributes;
            Values = values;
        }

        public IQueryConstraint ToQueryConstraint()
        {
            Validate();
            if (Values != null)
            {
                return new SetQueryConstraint(Category, new HashSet<string>(Values));
            }
            var lower = Attributes[Lower];
            var upper = Attributes[Upper];
            if (lower != null && upper != null)
            {
                return new RangeQueryConstraint(Category, lower, upper);
            }
            return new UnboundedRangeQueryConstraint(Category);
        }

        private void Validate()
        {
            if (Category == null || Attributes == null)
            {
                throw new InvalidWireConstraint("Missing category");
            }
            if (Values != null)
            {
                if (Attributes.ContainsKey(Lower) || Attributes.ContainsKey(Upper))
                {
                    throw new InvalidWireConstraint("Contains values AND range");
                }
            }
            if ((! Attributes.ContainsKey(Lower)) && Attributes.ContainsKey(Upper))
            {
                throw new InvalidWireConstraint("Incomplete bounds");
            }
        }

        private const string Lower = "lower";
        private const string Upper = "upper";
    }

    public class InvalidWireConstraint : Exception
    {
        public InvalidWireConstraint(string message) : base(message)
        {
        }
    }
}
