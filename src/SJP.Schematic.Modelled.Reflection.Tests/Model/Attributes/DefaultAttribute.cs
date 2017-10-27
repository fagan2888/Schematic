﻿using System;
using System.Globalization;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    public class DefaultAttributeTests
    {
        [Test]
        public void Ctor_GivenNullStringValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultAttribute(null));
        }

        [Test]
        public void DefaultValue_PropertyGetFromStringCtorArg_MatchesCtorArgument()
        {
            const string defaultValue = "test";
            var defaultAttr = new DefaultAttribute(defaultValue);

            Assert.AreEqual(defaultValue, defaultAttr.DefaultValue);
        }

        [Test]
        public void DefaultValue_PropertyGetFromDoubleCtorArg_MatchesCtorArgument()
        {
            const double defaultValue = 3.1415926;
            var defaultAttr = new DefaultAttribute(defaultValue);

            Assert.AreEqual(defaultValue.ToString(CultureInfo.InvariantCulture), defaultAttr.DefaultValue);
        }

        [Test]
        public void DefaultValue_PropertyGetFromIntCtorArg_MatchesCtorArgument()
        {
            const int defaultValue = 100;
            var defaultAttr = new DefaultAttribute(defaultValue);

            Assert.AreEqual(defaultValue.ToString(CultureInfo.InvariantCulture), defaultAttr.DefaultValue);
        }
    }
}