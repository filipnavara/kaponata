// <copyright file="NSDictionaryExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using System;
using System.Collections.Generic;

namespace Kaponata.iOS
{
    /// <summary>
    /// Provides extension methods for the <see cref="NSDictionary"/> class.
    /// </summary>
    public static class NSDictionaryExtensions
    {
        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>, as a list of <see cref="string"/> values.
        /// </summary>
        /// <param name="dict">
        /// The dictionary in which to search for the value associated with the specified <paramref name="key"/>.
        /// </param>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key, or <see langword="null"/> if the specified key is not found.
        /// </returns>
        public static IList<string> GetStringArray(this NSDictionary dict, string key)
        {
            if (!dict.ContainsKey(key))
            {
                return null;
            }

            List<string> values = new List<string>();

            switch (dict[key])
            {
                case NSArray array:
                    foreach (NSString value in array)
                    {
                        values.Add(value.Content);
                    }

                    break;

                case NSString value:
                    values.Add(value.Content);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(key));
            }

            return values;
        }

        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>, as a <see cref="string"/> value.
        /// </summary>
        /// <param name="dict">
        /// The dictionary in which to search for the value associated with the specified <paramref name="key"/>.
        /// </param>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key, or <see langword="null"/> if the specified key is not found.
        /// </returns>
        public static string GetString(this NSDictionary dict, string key)
        {
            if (!dict.ContainsKey(key))
            {
                return null;
            }

            return ((NSString)dict[key]).Content;
        }

        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>, as nullable <see langword="bool"/> value.
        /// </summary>
        /// <param name="dict">
        /// The dictionary in which to search for the value associated with the specified <paramref name="key"/>.
        /// </param>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key, or <see langword="null"/> if the specified key is not found.
        /// </returns>
        public static bool? GetNullableBoolean(this NSDictionary dict, string key)
        {
            if (!dict.ContainsKey(key))
            {
                return null;
            }

            return ((NSNumber)dict[key]).ToBool();
        }

        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>, as a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="dict">
        /// The dictionary in which to search for the value associated with the specified <paramref name="key"/>.
        /// </param>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key.
        /// </returns>
        public static DateTimeOffset GetDateTime(this NSDictionary dict, string key)
        {
            var date = ((NSDate)dict[key]).Date;
            return new DateTimeOffset(date.ToUniversalTime());
        }

        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>, as a <see cref="int"/> value.
        /// </summary>
        /// <param name="dict">
        /// The dictionary in which to search for the value associated with the specified <paramref name="key"/>.
        /// </param>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key.
        /// </returns>
        public static int GetInt32(this NSDictionary dict, string key)
        {
            return ((NSNumber)dict[key]).ToInt();
        }

        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>, as a <see cref="NSDictionary"/> value.
        /// </summary>
        /// <param name="dict">
        /// The dictionary in which to search for the value associated with the specified <paramref name="key"/>.
        /// </param>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key, or <see langword="null"/> if the specified key is not found.
        /// </returns>
        public static NSDictionary GetDict(this NSDictionary dict, string key)
        {
            if (!dict.ContainsKey(key))
            {
                return null;
            }

            return (NSDictionary)dict[key];
        }

        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>, as a <see cref="byte"/> array value.
        /// </summary>
        /// <param name="dict">
        /// The dictionary in which to search for the value associated with the specified <paramref name="key"/>.
        /// </param>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key, or <see langword="null"/> if the specified key is not found.
        /// </returns>
        public static byte[] GetData(this NSDictionary dict, string key)
        {
            if (!dict.ContainsKey(key))
            {
                return null;
            }

            return ((NSData)dict[key]).Bytes;
        }

        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>, as a list of <see cref="byte"/> array values.
        /// </summary>
        /// <param name="dict">
        /// The dictionary in which to search for the value associated with the specified <paramref name="key"/>.
        /// </param>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key, or <see langword="null"/> if the specified key is not found.
        /// </returns>
        public static IList<byte[]> GetDataArray(this NSDictionary dict, string key)
        {
            if (!dict.ContainsKey(key))
            {
                return null;
            }

            List<byte[]> value = new List<byte[]>();

            var array = (NSArray)dict[key];
            foreach (NSData entry in array)
            {
                value.Add(entry.Bytes);
            }

            return value;
        }
    }
}
