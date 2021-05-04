// <copyright file="AppleVersionTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Kaponata.iOS.Tests
{
    /// <summary>
    /// Tests the <see cref="AppleVersion"/> class.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312", Justification = "Starts with numbers", Scope = "type")]
    public class AppleVersionTests
    {
        /// <summary>
        /// Tests the <see cref="AppleVersion.AppleVersion(int, char, int)"/> constructor.
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            var version = new AppleVersion(13, 'A', 340);
            Assert.Equal(13, version.Major);
            Assert.Equal('A', version.Minor);
            Assert.Equal(340, version.Build);
            Assert.Null(version.Revision);
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.AppleVersion(int, char, int, int, char?)"/> constructor.
        /// </summary>
        [Fact]
        public void ConstructorTest2()
        {
            var version = new AppleVersion(13, 'A', 4, 325, 'c');
            Assert.Equal(13, version.Major);
            Assert.Equal('A', version.Minor);
            Assert.Equal(4, version.Architecture);
            Assert.Equal(325, version.Build);
            Assert.Equal('c', version.Revision);
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.AppleVersion(int, char, int)"/> constructor
        /// with a lower case minor verison number.
        /// </summary>
        [Fact]
        public void ConstructorTest3()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AppleVersion(13, 'a', 340));
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.CompareTo(object)"/> method.
        /// </summary>
        [Fact]
        public void CompareToObjectTest()
        {
            var _8k2 = new AppleVersion(8, 'K', 2);
            Assert.Equal(1, _8k2.CompareTo((object)null));
            Assert.Equal(0, _8k2.CompareTo((object)_8k2));
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.CompareTo(AppleVersion)"/> method.
        /// </summary>
        [Fact]
        public void CompareToAppleVersionTest()
        {
            var _8k2 = new AppleVersion(8, 'K', 2);
            var _13a340 = new AppleVersion(13, 'A', 340);
            var _13a344 = new AppleVersion(13, 'A', 344);
            var _13a344a = new AppleVersion(13, 'A', 1, 344, 'a');
            var _13a344b = new AppleVersion(13, 'A', 1, 344, 'b');
            var _13a344bis = new AppleVersion(13, 'A', 344);
            var _13b2 = new AppleVersion(13, 'B', 2);

            Assert.Equal(1, _8k2.CompareTo((AppleVersion)null));

            // Compare on major
            Assert.Equal(-1, _8k2.CompareTo(_13a340));
            Assert.Equal(1, _13a340.CompareTo(_8k2));

            // Compare on minor
            Assert.Equal(-1, _13a340.CompareTo(_13b2));
            Assert.Equal(1, _13b2.CompareTo(_13a340));

            // Compare on build
            Assert.Equal(1, _13a344.CompareTo(_13a340));
            Assert.Equal(-1, _13a340.CompareTo(_13a344));

            // Compare on revision
            Assert.Equal(1, _13a344.CompareTo(_13a344a));
            Assert.Equal(-1, _13a344a.CompareTo(_13a344));
            Assert.Equal(0, _13a344a.CompareTo(_13a344a));
            Assert.Equal(-1, _13a344a.CompareTo(_13a344b));
            Assert.Equal(1, _13a344b.CompareTo(_13a344a));

            // Compare equals
            Assert.Equal(0, _13a344.CompareTo(_13a344bis));
        }

        /// <summary>
        /// Tests the parsing of the iOS 9 versions.
        /// </summary>
        [Fact]
        public void ParseiOS9VersionsTest()
        {
            var beta = AppleVersion.Parse("13A4254v");
            var beta2 = AppleVersion.Parse("13A4280e");
            var beta3 = AppleVersion.Parse("13A4293g");
            var beta4 = AppleVersion.Parse("13A4305g");
            var beta5 = AppleVersion.Parse("13A4325c");
            var rtm = AppleVersion.Parse("13A340");

            Assert.Equal("13A4254v", beta.ToString());
            Assert.Equal("13A4280e", beta2.ToString());
            Assert.Equal("13A4293g", beta3.ToString());
            Assert.Equal("13A4305g", beta4.ToString());
            Assert.Equal("13A4325c", beta5.ToString());
            Assert.Equal("13A340", rtm.ToString());

            Assert.Equal(13, beta.Major);
            Assert.Equal('A', beta.Minor);
            Assert.Equal(4, beta.Architecture);
            Assert.Equal(254, beta.Build);
            Assert.Equal('v', beta.Revision);

            Assert.Equal(13, beta2.Major);
            Assert.Equal('A', beta2.Minor);
            Assert.Equal(4, beta2.Architecture);
            Assert.Equal(280, beta2.Build);
            Assert.Equal('e', beta2.Revision);

            Assert.Equal(13, beta3.Major);
            Assert.Equal('A', beta3.Minor);
            Assert.Equal(4, beta3.Architecture);
            Assert.Equal(293, beta3.Build);
            Assert.Equal('g', beta3.Revision);

            Assert.Equal(13, beta4.Major);
            Assert.Equal('A', beta4.Minor);
            Assert.Equal(4, beta4.Architecture);
            Assert.Equal(305, beta4.Build);
            Assert.Equal('g', beta4.Revision);

            Assert.Equal(13, beta5.Major);
            Assert.Equal('A', beta5.Minor);
            Assert.Equal(4, beta5.Architecture);
            Assert.Equal(325, beta5.Build);
            Assert.Equal('c', beta5.Revision);

            Assert.Equal(13, rtm.Major);
            Assert.Equal('A', rtm.Minor);
            Assert.Equal(0, rtm.Architecture);
            Assert.Equal(340, rtm.Build);
            Assert.Null(rtm.Revision);
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.CompareTo(AppleVersion)"/> method
        /// for the various beta versions of iOS 9.
        /// </summary>
        [Fact]
        public void CompareiOS9VersionsTest()
        {
            var beta = AppleVersion.Parse("13A4254v");
            var beta2 = AppleVersion.Parse("13A4280e");
            var beta3 = AppleVersion.Parse("13A4293g");
            var beta4 = AppleVersion.Parse("13A4305g");
            var beta5 = AppleVersion.Parse("13A4325c");
            var rtm = AppleVersion.Parse("13A340");

            Assert.True(beta < beta2);
            Assert.True(beta2 < beta3);
            Assert.True(beta3 < beta4);
            Assert.True(beta4 < beta5);
            Assert.True(beta5 < rtm);

            Assert.True(beta < rtm);
            Assert.True(beta2 < rtm);
            Assert.True(beta3 < rtm);
            Assert.True(beta4 < rtm);
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.Clone"/> method.
        /// </summary>
        [Fact]
        public void CloneTest()
        {
            var _8k2 = new AppleVersion(8, 'K', 2);
            var clone = _8k2.Clone();

            Assert.IsType<AppleVersion>(clone);

            var version = (AppleVersion)clone;
            Assert.Equal(8, version.Major);
            Assert.Equal('K', version.Minor);
            Assert.Equal(2, version.Build);
            Assert.Equal(0, version.Architecture);
            Assert.Null(version.Revision);
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.Equals(AppleVersion)"/> and
        /// <see cref="AppleVersion.Equals(object)"/> methods.
        /// </summary>
        [Fact]
        public void EqualsTest()
        {
            var _8k2 = new AppleVersion(8, 'K', 2);
            var _8k2bis = new AppleVersion(8, 'K', 2);
            var _8k3 = new AppleVersion(8, 'K', 3);
            var _8l2 = new AppleVersion(8, 'L', 2);
            var _9k2 = new AppleVersion(9, 'K', 2);

            // Test for equality
            Assert.Equal(_8k2, _8k2bis);
            Assert.Equal(_8k2bis, _8k2);

            // Test for equality - differ on major
            Assert.NotEqual(_8k2, _9k2);
            Assert.NotEqual(_9k2, _8k2);

            // Test for equality - differ on minor
            Assert.NotEqual(_8k2, _8l2);
            Assert.NotEqual(_8l2, _8k2);

            // Test for equality - differ on build
            Assert.NotEqual(_8k2, _8k3);
            Assert.NotEqual(_8k3, _8k2);

            // Test for equality - with null
            Assert.False(_8k2.Equals((object)null));
            Assert.True(_8k2.Equals((object)_8k2));
            Assert.NotEqual(_8k2, (object)null);
            Assert.NotEqual(_8k2, (AppleVersion)null);
            Assert.NotEqual((object)null, _8k2);
            Assert.NotEqual((AppleVersion)null, _8k2);
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.GetHashCode"/> method.
        /// </summary>
        [Fact]
        public void GetHashcodeTest()
        {
            var _8k2 = new AppleVersion(8, 'K', 2);

            Assert.Equal(0x084B0200, _8k2.GetHashCode());
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.ToString"/> method.
        /// </summary>
        [Fact]
        public void ToStringTest()
        {
            var _8k2 = new AppleVersion(8, 'K', 2);
            Assert.Equal("8K2", _8k2.ToString());
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion"/> operators.
        /// </summary>
        [Fact]
        public void OperatorTest()
        {
            var _8k2 = new AppleVersion(8, 'K', 2);
            var _8k2bis = new AppleVersion(8, 'K', 2);
            var _8k3 = new AppleVersion(8, 'K', 3);

            // == operator
            Assert.True(_8k2 == _8k2bis);
            Assert.True(!(_8k2 == _8k3));

            // != operatior
            Assert.True(!(_8k2 != _8k2bis));
            Assert.True(_8k2 != _8k3);

            // < operator
            Assert.True(!(_8k2 < _8k2bis));
            Assert.True(_8k2 < _8k3);
            Assert.True(!(_8k3 < _8k2));

            // > operator
            Assert.True(!(_8k2 > _8k2bis));
            Assert.True(!(_8k2 > _8k3));
            Assert.True(_8k3 > _8k2);

            // <= operator
            Assert.True(_8k2 <= _8k2bis);
            Assert.True(_8k2 <= _8k3);
            Assert.False(_8k3 <= _8k2);

            // >= operator
            Assert.True(_8k2 >= _8k2bis);
            Assert.False(_8k2 >= _8k3);
            Assert.True(_8k3 >= _8k2);
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.Parse(string)"/> method.
        /// </summary>
        [Fact]
        public void ParseVersionTest()
        {
            var version = AppleVersion.Parse("13A340");
            Assert.Equal(13, version.Major);
            Assert.Equal('A', version.Minor);
            Assert.Equal(340, version.Build);
            Assert.Equal(0, version.Architecture);
            Assert.Null(version.Revision);

            version = AppleVersion.Parse("13A4305g");
            Assert.Equal(13, version.Major);
            Assert.Equal('A', version.Minor);
            Assert.Equal(305, version.Build);
            Assert.Equal(4, version.Architecture);
            Assert.Equal('g', version.Revision);

            version = AppleVersion.Parse("8K2");
            Assert.Equal(8, version.Major);
            Assert.Equal('K', version.Minor);
            Assert.Equal(2, version.Build);
            Assert.Equal(0, version.Architecture);
            Assert.Null(version.Revision);
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.Parse(string)"/> method with a
        /// <see langword="null"/> argument.
        /// </summary>
        [Fact]
        public void ParseVersionNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => AppleVersion.Parse(null));
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.Parse(string)"/> method with a
        /// empty string as an argument.
        /// </summary>
        [Fact]
        public void ParseEmptyVersionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AppleVersion.Parse(string.Empty));
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.Parse(string)"/> method with a
        /// an invalid version number.
        /// </summary>
        [Fact]
        public void ParseInvalidVersionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AppleVersion.Parse("8KK2"));
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.Parse(string)"/> method with a
        /// an invalid version number.
        /// </summary>
        [Fact]
        public void ParseInvalidVersionTest2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AppleVersion.Parse("K82"));
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.Parse(string)"/> method with a
        /// an invalid version number.
        /// </summary>
        [Fact]
        public void ParseInvalidVersionTest3()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AppleVersion.Parse("82K"));
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion.DifferByBuildNumberOnly(AppleVersion, AppleVersion)"/> method.
        /// </summary>
        [Fact]
        public void DifferByBuildNumberOnlyTests()
        {
            // iOS 9.3 == iOS 9.3
            Assert.True(AppleVersion.DifferByBuildNumberOnly(AppleVersion.Parse("13E230"), AppleVersion.Parse("13E230")));

            // iOS 9.3.1 == iOS 9.3
            Assert.True(AppleVersion.DifferByBuildNumberOnly(AppleVersion.Parse("13E230"), AppleVersion.Parse("13E238")));

            // iOS 9.0 beta 5 != iOS 9.0 RTM
            Assert.True(!AppleVersion.DifferByBuildNumberOnly(AppleVersion.Parse("13A340"), AppleVersion.Parse("13A4325c")));

            // Null tests for code coverage
            Assert.True(!AppleVersion.DifferByBuildNumberOnly(AppleVersion.Parse("13E230"), null));
            Assert.True(!AppleVersion.DifferByBuildNumberOnly(null, AppleVersion.Parse("13E230")));
            Assert.True(!AppleVersion.DifferByBuildNumberOnly(null, null));
        }

        /// <summary>
        /// Additional tests for parsing the iOS 12 version number.
        /// </summary>
        /// <param name="versionString">
        /// The iOS 12 version string.
        /// </param>
        /// <param name="expectedBuild">
        /// The expected build number.
        /// </param>
        /// <param name="expectedRevision">
        /// The expected revision.
        /// </param>
        [Theory]
        [InlineData("16A5366a", 366, 'a')]
        [InlineData("16A5365b", 365, 'b')]
        [InlineData("16A5364a", 364, 'a')]
        [InlineData("16A5362a", 362, 'a')]
        [InlineData("16A5357b", 357, 'b')]
        [InlineData("16A5354b", 354, 'b')]
        [InlineData("16A5345f", 345, 'f')]
        [InlineData("16A5339e", 339, 'e')]
        [InlineData("16A5327f", 327, 'f')]
        [InlineData("16A5318d", 318, 'd')]
        [InlineData("16A5308e", 308, 'e')]
        [InlineData("16A5288q", 288, 'q')]
        public void ParseiOS12VersionTest(string versionString, int expectedBuild, char expectedRevision)
        {
            var version = AppleVersion.Parse(versionString);
            Assert.Equal(16, version.Major);
            Assert.Equal('A', version.Minor);
            Assert.Equal(5, version.Architecture);
            Assert.Equal(expectedBuild, version.Build);
            Assert.Equal(expectedRevision, version.Revision);
            Assert.Equal(versionString, version.ToString());
        }

        /// <summary>
        /// Tests the <see cref="AppleVersion"/> constructor with invalid values.
        /// </summary>
        /// <param name="major">
        /// The major version number.
        /// </param>
        /// <param name="minor">
        /// The minor version number.
        /// </param>
        /// <param name="architecture">
        /// The target architecture.
        /// </param>
        /// <param name="build">
        /// The build number.
        /// </param>
        /// <param name="revision">
        /// The revision number.
        /// </param>
        [Theory]
        [InlineData(-1, 'A', 5, 300, 'f')]
        [InlineData(1, 'A', -1, 300, 'f')]
        [InlineData(1, 'A', 11, 300, 'f')]
        [InlineData(1, 'A', 5, -1, 'f')]
        [InlineData(1, 'A', 5, 1001, 'f')]
        [InlineData(1, 'A', 5, 300, 'F')]
        [InlineData(1, 'A', 5, 300, '?')]
        public void ConstructorInvalidValuesTest(int major, char minor, int architecture, int build, char? revision)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AppleVersion(major, minor, architecture, build, revision));
        }

        /// <summary>
        /// Tests the various operators, with a focus on <see langword="null"/> values.
        /// </summary>
        [Fact]
        public void OperatorTest2()
        {
            Assert.True(AppleVersion.Parse("16A5366a") < AppleVersion.Parse("16A5366b"));
            Assert.False(AppleVersion.Parse("16A5366b") < null);
            Assert.False((AppleVersion)null < null);

            Assert.True(AppleVersion.Parse("16A5366a") <= AppleVersion.Parse("16A5366b"));
            Assert.False(AppleVersion.Parse("16A5366b") <= null);
            Assert.True((AppleVersion)null <= null);

            Assert.False(AppleVersion.Parse("16A5366a") > AppleVersion.Parse("16A5366b"));
            Assert.True(AppleVersion.Parse("16A5366b") > null);
            Assert.False((AppleVersion)null > null);

            Assert.False(AppleVersion.Parse("16A5366a") >= AppleVersion.Parse("16A5366b"));
            Assert.True(AppleVersion.Parse("16A5366a") >= null);
            Assert.True((AppleVersion)null >= null);
        }
    }
}
