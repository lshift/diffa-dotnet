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
  /// Category function that takes the initial <code>length</code> characters of a string.
  /// </summary>
  public class PrefixCategoryFunction : BaseCategoryFunction {
    public int Length { get; private set; }

    public PrefixCategoryFunction(string attributeName, int length) : base(attributeName) {
      Length = length;
    }

    public override string Name {
      get { return "prefix(" + Length + ")"; }
    }

    public override string OwningPartition(string value) {
      return value.Substring(0, Length);
    }
  }
}
