﻿//
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
    class IntegerCategoryFunctionTest
    {
        [Test]
        public void OwningPartitionNameShouldEqualBaseOfPartitionRange()
        {
            var categoryFunction = new IntegerCategoryFunction("someInt", 100);
            Assert.AreEqual("200", categoryFunction.OwningPartition("234"));
        }

        [Test]
        [ExpectedException(typeof (InvalidAttributeValueException))]
        public void ShouldThrowInvalidAttributeExceptionWhenValueDoesNotParseToInteger()
        {
            var categoryFunction = new IntegerCategoryFunction("someInt", 100);
            categoryFunction.OwningPartition("NOT_AN_INTEGER");
        }

        [Test]
        public void NameShouldBeBasedOnDenominator()
        {
            var categoryFunction = new IntegerCategoryFunction("someInt", 100);
            Assert.AreEqual("100s", categoryFunction.Name);
        }
    }
}
