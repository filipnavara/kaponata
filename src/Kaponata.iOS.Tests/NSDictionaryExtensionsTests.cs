// <copyright file="NSDictionaryExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kaponata.iOS.Tests
{
    /// <summary>
    /// Tests the <see cref="NSDictionaryExtensions"/> class.
    /// </summary>
    public class NSDictionaryExtensionsTests
    {
        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetStringArray(NSDictionary, string)"/> returns <see langword="null"/>
        /// if the key is not present in the dictionary.
        /// </summary>
        [Fact]
        public void GetStringArray_MissingKey_ReturnsNull()
        {
            var dict = new NSDictionary();
            Assert.Null(dict.GetStringArray("missing"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetStringArray(NSDictionary, string)"/> returns the correct value
        /// if the entry is a single <see cref="NSString"/> object.
        /// </summary>
        [Fact]
        public void GetStringArray_NSString_ReturnsValue()
        {
            var dict = new NSDictionary();
            dict.Add("foo", new NSString("bar"));

            var value = Assert.Single(dict.GetStringArray("foo"));
            Assert.Equal("bar", value);
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetStringArray(NSDictionary, string)"/> throws an exception when
        /// the array contains values which are not strings.
        /// </summary>
        [Fact]
        public void GetStringArray_NSArrayOfInvalid_ReturnsValue()
        {
            var dict = new NSDictionary();
            dict.Add("foo", new NSNumber(1));

            Assert.Throws<ArgumentOutOfRangeException>(() => dict.GetStringArray("foo"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetStringArray(NSDictionary, string)"/> returns the correct value
        /// if the entry is a <see cref="NSArray"/> of <see cref=" NSString"/> objects.
        /// </summary>
        [Fact]
        public void GetStringArray_NSArray_ReturnsValue()
        {
            var array = new NSArray();
            array.Add(new NSString("value1"));
            array.Add(new NSString("value2"));

            var dict = new NSDictionary();
            dict.Add("key", array);

            Assert.Collection(
                dict.GetStringArray("key"),
                (v) => Assert.Equal("value1", v),
                (v) => Assert.Equal("value2", v));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetString(NSDictionary, string)"/> returns <see langword="null"/>
        /// if the key is not present in the dictionary.
        /// </summary>
        [Fact]
        public void GetString_MissingKey_ReturnsNull()
        {
            var dict = new NSDictionary();
            Assert.Null(dict.GetString("missing"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetString(NSDictionary, string)"/> returns the correct value
        /// if the entry is a single <see cref="NSString"/> object.
        /// </summary>
        [Fact]
        public void GetString_ReturnsValue()
        {
            var dict = new NSDictionary();
            dict.Add("key", new NSString("value"));

            Assert.Equal("value", dict.GetString("key"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetNullableBoolean(NSDictionary, string)"/> returns <see langword="null"/>
        /// if the key is not present in the dictionary.
        /// </summary>
        [Fact]
        public void GetNullableBoolean_MissingKey_ReturnsNull()
        {
            var dict = new NSDictionary();
            Assert.Null(dict.GetNullableBoolean("missing"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetNullableBoolean(NSDictionary, string)"/> returns the correct value
        /// if the entry is a single <see cref="NSNumber"/> object.
        /// </summary>
        [Fact]
        public void GetNullableBoolean_ReturnsValue()
        {
            var dict = new NSDictionary();
            dict.Add("key", new NSNumber(true));

            Assert.True(dict.GetNullableBoolean("key"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetBoolean(NSDictionary, string)"/> throws an excpetion
        /// if the key is not present in the dictionary.
        /// </summary>
        [Fact]
        public void GetBoolean_MissingKey_Throws()
        {
            var dict = new NSDictionary();
            Assert.Throws<KeyNotFoundException>(() => dict.GetBoolean("missing"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetBoolean(NSDictionary, string)"/> returns the correct value
        /// if the entry is a single <see cref="NSNumber"/> object.
        /// </summary>
        [Fact]
        public void GetBoolean_ReturnsValue()
        {
            var dict = new NSDictionary();
            dict.Add("key", new NSNumber(true));

            Assert.True(dict.GetBoolean("key"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetDateTime(NSDictionary, string)"/> returns the correct value
        /// if the entry is a single <see cref="NSDate"/> object.
        /// </summary>
        [Fact]
        public void GetDateTime_ReturnsValue()
        {
            var value = new DateTime(2000, 1, 1);

            var dict = new NSDictionary();
            dict.Add("key", new NSDate(value));

            Assert.Equal(value, dict.GetDateTime("key"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetInt32(NSDictionary, string)"/> returns the correct value
        /// if the entry is a single <see cref="NSDate"/> object.
        /// </summary>
        [Fact]
        public void GetInt32_ReturnsValue()
        {
            var dict = new NSDictionary();
            dict.Add("key", new NSNumber(42));

            Assert.Equal(42, dict.GetInt32("key"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetNullableInt32(NSDictionary, string)"/> returns the correct value
        /// if the entry is a single <see cref="NSNumber"/> object.
        /// </summary>
        [Fact]
        public void GetNullabelInt32_ReturnsValue()
        {
            var dict = new NSDictionary();
            dict.Add("key", new NSNumber(42));

            Assert.Equal(42, dict.GetNullableInt32("key"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetNullableInt32(NSDictionary, string)"/> returns <see langword="null"/>
        /// if the key is not present in the dictionary.
        /// </summary>
        [Fact]
        public void GetNullabelInt32_MissingKey_ReturnsNull()
        {
            var dict = new NSDictionary();

            Assert.Null(dict.GetNullableInt32("key"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetDict(NSDictionary, string)"/> returns <see langword="null"/>
        /// if the key is not present in the dictionary.
        /// </summary>
        [Fact]
        public void GetDict_MissingKey_ReturnsNull()
        {
            var dict = new NSDictionary();
            Assert.Null(dict.GetDict("missing"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetDict(NSDictionary, string)"/> returns the correct value
        /// if the entry is a single <see cref="NSDictionary"/> object.
        /// </summary>
        [Fact]
        public void GetDict_ReturnsValue()
        {
            var child = new NSDictionary();
            var dict = new NSDictionary();
            dict.Add("key", child);

            Assert.Equal(child, dict.GetDict("key"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetDataArray(NSDictionary, string)"/> returns <see langword="null"/>
        /// if the key is not present in the dictionary.
        /// </summary>
        [Fact]
        public void GetDataArray_MissingKey_ReturnsNull()
        {
            var dict = new NSDictionary();
            Assert.Null(dict.GetDataArray("missing"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetDataArray(NSDictionary, string)"/> returns the correct value
        /// if the entry is a <see cref="NSArray"/> of <see cref="NSData"/> values.
        /// </summary>
        [Fact]
        public void GetDataArray_ReturnsValue()
        {
            var entry = new byte[] { 1, 2, 3, 4 };
            var array = new NSArray();
            array.Add(new NSData(entry));

            var dict = new NSDictionary();
            dict.Add("key", array);

            var value = Assert.Single(dict.GetDataArray("key"));
            Assert.Equal(entry, value);
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetData(NSDictionary, string)"/> returns <see langword="null"/>
        /// if the key is not present in the dictionary.
        /// </summary>
        [Fact]
        public void GetData_MissingKey_ReturnsNull()
        {
            var dict = new NSDictionary();
            Assert.Null(dict.GetData("missing"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetData(NSDictionary, string)"/> returns the correct value
        /// if the entry is a <see cref="NSArray"/> of <see cref="NSData"/> values.
        /// </summary>
        [Fact]
        public void GetData_ReturnsValue()
        {
            var entry = new byte[] { 1, 2, 3, 4 };
            var dict = new NSDictionary();
            dict.Add("key", new NSData(entry));

            var value = dict.GetData("key");
            Assert.Equal(entry, value);
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetDictionary(NSDictionary, string)"/> return <see langword="null"/>
        /// when a value with the requested key could not be found.
        /// </summary>
        [Fact]
        public void GetDictionary_MissingKey_ReturnsNull()
        {
            var dict = new NSDictionary();
            Assert.Null(dict.GetDictionary("missing"));
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.GetDictionary(NSDictionary, string)"/> returns the correct value
        /// when a value with the requested key exists.
        /// </summary>
        [Fact]
        public void GetDictionary_ReturnsValue()
        {
            var dict = new NSDictionary();
            var entry = new NSDictionary();
            entry.Add("A", "a");
            entry.Add("1", 1);
            dict.Add("entry", entry);

            var value = dict.GetDictionary("entry");

            Assert.Collection(
                value,
                p =>
                {
                    Assert.Equal("A", p.Key);
                    Assert.Equal("a", p.Value);
                },
                p =>
                {
                    Assert.Equal("1", p.Key);
                    Assert.Equal(1, p.Value);
                });
        }

        /// <summary>
        /// <see cref="NSDictionaryExtensions.AddWhenNotNull(NSDictionary, string, object)"/> works correctly.
        /// </summary>
        [Fact]
        public void AddWhenNotNull_Works()
        {
            var dict = new NSDictionary();
            dict.AddWhenNotNull("null", null);
            dict.AddWhenNotNull("notnull", string.Empty);

            var key = Assert.Single(dict.Keys);
            Assert.Equal("notnull", key);
        }
    }
}
