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
using NUnit.Framework;

namespace Net.LShift.Diffa.Participants.Test {
  [TestFixture]
  public class AggregationBuilderTest {
    [Test]
    public void ShouldNotAddDateAggregationForEmptyRequest() {
      var req = new NameValueCollection();
      var builder = new AggregationBuilder(req);

      builder.MaybeAddDateAggregation("test");
      Assert.AreEqual(0, builder.ToList().Count);
    }

    [Test]
    public void ShouldAddDateAggregationWhenParameterIsAvailable() {
      var req = new NameValueCollection();
      req.Add("bizDate-granularity", "monthly");
      var builder = new AggregationBuilder(req);

      builder.MaybeAddDateAggregation("bizDate");
      Assert.AreEqual(1, builder.ToList().Count);
      Assert.That(builder.ToList()[0], Is.InstanceOf(typeof(DateCategoryFunction)));
    }

    [Test]
    public void ShouldNotAddDateAggregationWhenDifferentParameterIsAvailable() {
      var req = new NameValueCollection();
      req.Add("someString-granularity", "prefix(1)");
      var builder = new AggregationBuilder(req);

      builder.MaybeAddDateAggregation("bizDate");
      Assert.AreEqual(0, builder.ToList().Count);
    }

    [Test]
    public void ShouldNotAddNyNameAggregationForEmptyRequest() {
      var req = new NameValueCollection();
      var builder = new AggregationBuilder(req);

      builder.MaybeAddByNameAggregation("test");
      Assert.AreEqual(0, builder.ToList().Count);
    }

    [Test]
    public void ShouldAddByNameAggregationWhenParameterIsAvailable() {
      var req = new NameValueCollection();
      req.Add("someString-granularity", "by-name");
      var builder = new AggregationBuilder(req);

      builder.MaybeAddByNameAggregation("someString");
      Assert.AreEqual(1, builder.ToList().Count);
      Assert.That(builder.ToList()[0], Is.InstanceOf(typeof(ByNameCategoryFunction)));
    }
  }
}
