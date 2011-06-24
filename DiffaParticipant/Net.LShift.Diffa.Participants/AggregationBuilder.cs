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
using System.Linq;
using System.Text;

namespace Net.LShift.Diffa.Participants {
  /// <summary>
  /// Helper for building aggregations from a web request.
  /// </summary>
  public class AggregationBuilder {
    private readonly NameValueCollection req;
    private readonly List<ICategoryFunction> result;

    public AggregationBuilder(NameValueCollection req) {
      this.req = req;
      this.result = new List<ICategoryFunction>();
    }

    /// <summary>
    /// Transforms the builder into a list of category functions.
    /// </summary>
    /// <returns>the category functions</returns>
    public List<ICategoryFunction> ToList() {
      return result;
    }

    /// <summary>
    /// Attempt to add a date aggregation for the given attribute. The aggregation will be added if
    /// [attrName]-granularity is present in the request.
    /// </summary>
    /// <param name="attrName">the name of the attribute</param>
    public void MaybeAddDateAggregation(string attrName) {
      var attrGranularity = req[attrName + "-granularity"];
      if (attrGranularity != null) {
        switch (attrGranularity) {
          case "daily": 
            result.Add(new DailyCategoryFunction(attrName));
            break;
          case "monthly": 
            result.Add(new MonthlyCategoryFunction(attrName));
            break;
          case "yearly":
            result.Add(new YearlyCategoryFunction(attrName));
            break;
        }
      }
    }

    /// <summary>
    /// Attempt to add a by name aggregation for the given attribute.
    /// </summary>
    /// <param name="attrName">the name of the attribute</param>
    public void MaybeAddByNameAggregation(string attrName) {
      var attrGranularity = req[attrName + "-granularity"];
      if (attrGranularity != null && attrGranularity == "by-name") {
        result.Add(new ByNameCategoryFunction(attrName));
      }
    }
  }
}
