// <copyright file="Selector.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Provides helper methods for working with Kubernetes selector strings.
    /// </summary>
    public static class Selector
    {
        /// <summary>
        /// Converts a dictionary which contains selector keys and values to a selector string.
        /// </summary>
        /// <param name="values">
        /// A dictionary which contains selector keys and values to a selector string.
        /// </param>
        /// <returns>
        /// The equivalent selector string.
        /// </returns>
        public static string Create(Dictionary<string, string> values)
        {
            if (values == null || values.Count == 0)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();

            foreach (var pair in values)
            {
                if (builder.Length > 0)
                {
                    builder.Append(',');
                }

                builder.Append($"{pair.Key}={pair.Value}");
            }

            return builder.ToString();
        }
    }
}
