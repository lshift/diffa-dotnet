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
using System.Collections.Specialized;
using System.Globalization;

namespace Net.LShift.Diffa.Participants {
  /// <summary>
  /// Utility for building a list of constraints based on a set of name-value pairs describing a query request.
  /// </summary>
  public class ConstraintsBuilder {
    private readonly NameValueCollection _reqParams;
    private readonly List<IQueryConstraint> _result;

    public ConstraintsBuilder(NameValueCollection reqParams) {
      this._reqParams = reqParams;
      this._result = new List<IQueryConstraint>();
    }

    /**
     * Transforms the builder into a list of constraints.
     * @return the constraint list.
     */
    public List<IQueryConstraint> ToList() {
      return _result;
    }

    
    /// <summary>
    /// Attempt to add a date range constraint for the given attribute. The constraint will be added if
    /// one or both of [attrName]-start, [attrName]-end are present in the request.
    /// </summary>
    /// <param name="attrName">the name of the attribute</param>
    public void MaybeAddDateRangeConstraint(string attrName) {
      var startVal = RetrieveAndParseDateTimeBound(attrName + "-start");
      var endVal = RetrieveAndParseDateTimeBound(attrName + "-end");

      if (startVal != null || endVal != null) {
        _result.Add(new DateRangeQueryConstraint(attrName, startVal, endVal));
      }
    }

    /// <summary>
    /// Attempt to add a time range constraint for the given attribute. The constraint will be added if
    /// one or both of [attrName]-start, [attrName]-end are present in the request.
    /// </summary>
    /// <param name="attrName">the name of the attribute</param>
    public void MaybeAddTimeRangeConstraint(string attrName) {
      var startVal = RetrieveAndParseDateTimeBound(attrName + "-start");
      var endVal = RetrieveAndParseDateTimeBound(attrName + "-end");

      if (startVal != null || endVal != null) {
        _result.Add(new DateRangeQueryConstraint(attrName, startVal, endVal));
      }
    }

    /// <summary>
    /// Attempt to add a set constraint for the given attribute. The constraint will be added if there are
    /// any arguments in the request with the given attribute name.
    /// </summary>
    /// <param name="attrName">the name of the attribute</param>
    public void MaybeAddSetConstraint(string attrName) {
      string[] values = _reqParams.GetValues(attrName);

      if (values != null && values.Length > 0) {
        _result.Add(new SetQueryConstraint(attrName, new HashSet<string>(values)));
      }
    }

    /// <summary>
    /// Maybe retrieves a given field and parses a date/datetime query constraint bound.
    /// </summary>
    /// <param name="field">the name of the field being parsed</param>
    /// <returns>the parsed date, or null if the dateStr was null</returns>
    private DateTime? RetrieveAndParseDateTimeBound(string field) {
      string dateStr = _reqParams.Get(field);

      if (dateStr == null) {
        return null;
      }

      DateTime result;
      if (!DateTime.TryParse(dateStr, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out result))
        throw new InvalidConstraintException(field, dateStr);
      return result;
    }
  }
}
