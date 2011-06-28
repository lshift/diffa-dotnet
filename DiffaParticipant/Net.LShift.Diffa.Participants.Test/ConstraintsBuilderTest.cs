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
using NUnit.Framework;

namespace Net.LShift.Diffa.Participants.Test {
  [TestFixture]
  public class ConstraintsBuilderTest {
    [Test]
    public void ShouldNotAddDateRangeConstraintForEmptyRequest() {
      var req = new NameValueCollection();
      var builder = new ConstraintsBuilder(req);

      builder.MaybeAddDateRangeConstraint("test");
      Assert.AreEqual(0, builder.ToList().Count);
    }

    [Test]
    public void ShouldNotAddTimeRangeConstraintForEmptyRequest() {
      var req = new NameValueCollection();
      var builder = new ConstraintsBuilder(req);

      builder.MaybeAddTimeRangeConstraint("test");
      Assert.AreEqual(0, builder.ToList().Count);
    }

    [Test]
    public void ShouldNotAddSetConstraintForEmptyRequest() {
      var req = new NameValueCollection();
      var builder = new ConstraintsBuilder(req);

      builder.MaybeAddSetConstraint("test");
      Assert.AreEqual(0, builder.ToList().Count);
    }

    [Test]
    public void ShouldAddDateRangeConstraintWhenBothStartAndEndArePresent() {
      var req = new NameValueCollection();
      req["bizDate-start"] = "2011-06-01";
      req["bizDate-end"] = "2011-06-30";
      var builder = new ConstraintsBuilder(req);

      builder.MaybeAddDateRangeConstraint("bizDate");
      Assert.AreEqual(1, builder.ToList().Count);
      Assert.IsInstanceOf(typeof (DateRangeQueryConstraint), builder.ToList()[0]);

      var c = (DateRangeQueryConstraint) builder.ToList()[0];
      Assert.AreEqual(new DateTime(2011, 6, 1), c.LowerBound);
      Assert.AreEqual(new DateTime(2011, 6, 30), c.UpperBound);
      Assert.AreEqual("bizDate", c.Category);
    }

    [Test]
    public void ShouldAddTimeRangeConstraintWhenBothStartAndEndArePresent() {
      var req = new NameValueCollection();
      req["createTime-start"] = "2011-06-06T12:00:00.000Z";
      req["createTime-end"] = "2011-06-06T16:00:00.000Z";
      var builder = new ConstraintsBuilder(req);

      builder.MaybeAddTimeRangeConstraint("createTime");
      Assert.AreEqual(1, builder.ToList().Count);
      Assert.IsInstanceOf(typeof (DateRangeQueryConstraint), builder.ToList()[0]);

      var c = (DateRangeQueryConstraint) builder.ToList()[0];
      Assert.AreEqual(new DateTime(2011, 6, 6, 12, 0, 0, 0, DateTimeKind.Utc), c.LowerBound);
      Assert.AreEqual(new DateTime(2011, 6, 6, 16, 0, 0, 0, DateTimeKind.Utc), c.UpperBound);
    }

    [Test]
    public void ShouldAddSetConstraintWhenSingleValueIsPresent() {
      var req = new NameValueCollection();
      req["someString"] = "a";
      var builder = new ConstraintsBuilder(req);

      builder.MaybeAddSetConstraint("someString");
      Assert.AreEqual(1, builder.ToList().Count);
      Assert.IsInstanceOf(typeof (SetQueryConstraint), builder.ToList()[0]);

      var c = (SetQueryConstraint) builder.ToList()[0];
      var expected = new HashSet<String> {"a"};
      Assert.AreEqual(expected, c.Values);
    }

    [Test]
    public void ShouldAddSetConstraintWhenMultipleValuesArePresent() {
      var req = new NameValueCollection {{"someString", "a"}, {"someString", "b"}, {"someString", "c"}};
      var builder = new ConstraintsBuilder(req);

      builder.MaybeAddSetConstraint("someString");
      Assert.AreEqual(1, builder.ToList().Count);
      Assert.IsInstanceOf(typeof (SetQueryConstraint), builder.ToList()[0]);

      var c = (SetQueryConstraint) builder.ToList()[0];
      var expected = new HashSet<String> { "a", "b", "c" };
      Assert.That(expected, Is.EqualTo(c.Values).AsCollection);
    }

    [Test]
    public void ShouldBeAbleToAddBothSetAndDateConstraints()
    {
      var req = new NameValueCollection { { "someString", "a" }, { "bizDate-start", "2011-06-01" }, { "bizDate-end", "2011-06-30" } };
      var builder = new ConstraintsBuilder(req);

      builder.MaybeAddSetConstraint("someString");
      builder.MaybeAddDateRangeConstraint("bizDate");
      Assert.AreEqual(2, builder.ToList().Count);

      Assert.IsInstanceOf(typeof(SetQueryConstraint), builder.ToList()[0]);
      var sc = (SetQueryConstraint)builder.ToList()[0];
      Assert.That(new HashSet<String> { "a" }, Is.EqualTo(sc.Values).AsCollection);

      Assert.IsInstanceOf(typeof(DateRangeQueryConstraint), builder.ToList()[1]);
      var c = (DateRangeQueryConstraint)builder.ToList()[1];
      Assert.That(new DateTime(2011, 6, 1), Is.EqualTo(c.LowerBound.Value));
      Assert.That(new DateTime(2011, 6, 30), Is.EqualTo(c.UpperBound.Value));
    }
  }
}
