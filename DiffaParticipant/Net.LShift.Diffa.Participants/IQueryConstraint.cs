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
using System.Globalization;

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
    /// Represents a range of values as a sequence containing upper and lower bounds
    /// </summary>
    public class DateRangeQueryConstraint : RangeQueryConstraint
    {
      public new DateTime? LowerBound { get; private set; }
      public new DateTime? UpperBound { get; private set; }

      public DateRangeQueryConstraint(string attrName, string lowerBound, string upperBound) : this(attrName, ParseBound(lowerBound), ParseBound(upperBound)) {
      }

      public DateRangeQueryConstraint(string attrName, DateTime? lowerBound, DateTime? upperBound) :
          base(attrName, lowerBound != null ? lowerBound.Value.ToString("yyyy-MM-dd") : null, upperBound != null ? upperBound.Value.ToString("yyyy-MM-dd") : null) {
        LowerBound = lowerBound;
        UpperBound = upperBound;
      }

      public override string ToString()
      {
        return "RangeQueryConstraint(DataType=" + Category + ", LowerBound=" + LowerBound + ", UpperBound=" + UpperBound + ")";
      }

      public bool Includes(DateTime t) {
        if (LowerBound != null && t < LowerBound.Value) return false;
        if (UpperBound != null && t > UpperBound.Value) return false;
        return true;
      }

      private static DateTime? ParseBound(string s) {
        if (s == null) {
          return null;
        } else {
          return DateTime.Parse(s, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }
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

      public bool Includes(string val) {
        return Values.Contains(val);
      }
    }

    /// <summary>
    /// Represents a query constraint where a field's string value should start with the given prefix.
    /// </summary>
    public class PrefixQueryConstraint : IQueryConstraint {
        public string Category { get; private set; }
        public string Prefix { get; private set; }

        public PrefixQueryConstraint(string category, string prefix) {
            Category = category;
            Prefix = prefix;
        }

        public override string ToString() 
        {
          return "PrefixQueryConstraint(DataType=" + Category + ", Prefix=" + Prefix + ")";
        }

        public bool Includes(string val) {
          if (val == null) return false;

          return val.StartsWith(Prefix);
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

        /// <summary>
        /// Produce an IQueryConstraint, which is meaningful to a Participant, from the wire representation
        /// </summary>
        public IQueryConstraint ToQueryConstraint()
        {
            Validate();
            if (Values != null)
            {
                return new SetQueryConstraint(Category, new HashSet<string>(Values));
            }
            if (Attributes.ContainsKey(Lower) && Attributes.ContainsKey(Upper))
            {
                var lower = Attributes[Lower];
                var upper = Attributes[Upper];
                if (lower != null && upper != null)
                {
                    return new RangeQueryConstraint(Category, lower, upper);
                }
            }
            return new UnboundedRangeQueryConstraint(Category);
        }

        public override string ToString()
        {
            var attributes = String.Join(", ", Attributes);
            string values = null;
            if (null != Values)
            {
                values = String.Join(", ", Values);    
            }
            return string.Format("Category(Category={0},Attributes=[{1}],Values=[{2}])", Category, attributes, values);
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
