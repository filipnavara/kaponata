// <copyright file="LogFormatter.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kaponata.Operator.Tests
{
    /// <summary>
    /// Formats log messages.
    /// </summary>
    public class LogFormatter : ILogFormatter
    {
        /// <inheritdoc/>
        public string Format(
            int scopeLevel,
            string name,
            LogLevel logLevel,
            EventId eventId,
            string message,
            Exception exception)
        {
            const int ScopePaddingSpaces = 4;
            const string Format = "{0:O} {1}{2} [{3}]: {4}";
            var padding = new string(' ', scopeLevel * ScopePaddingSpaces);

            StringBuilder builder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(message) == false)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, Format, DateTime.Now, padding, logLevel, eventId.Id, message);
            }

            if (exception != null)
            {
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    Format,
                    DateTime.Now,
                    padding,
                    logLevel,
                    eventId.Id,
                    exception);
            }

            return builder.ToString();
        }
    }
}
