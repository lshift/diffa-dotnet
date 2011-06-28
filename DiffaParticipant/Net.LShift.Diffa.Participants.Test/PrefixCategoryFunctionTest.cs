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

using NUnit.Framework;

namespace Net.LShift.Diffa.Participants.Test
{
  [TestFixture]
  public class PrefixCategoryFunctionTest {
    [Test]
    public void ShouldPartitionStrings() {
      var f = new PrefixCategoryFunction("someString", 2);

      Assert.AreEqual("sa", f.OwningPartition("saw"));
      Assert.AreEqual("do", f.OwningPartition("dog"));
      Assert.AreEqual("ca", f.OwningPartition("cat"));
    }

    [Test]
    public void ShouldPrefixTooShortStringWithAllOfItsCharacters() {
      var f = new PrefixCategoryFunction("someString", 4);

      Assert.AreEqual("saw", f.OwningPartition("saw"));
      Assert.AreEqual("dog", f.OwningPartition("dog"));
      Assert.AreEqual("cat", f.OwningPartition("cat"));
    }
  }
}
