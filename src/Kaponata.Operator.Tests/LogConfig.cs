// <copyright file="LogConfig.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Divergic.Logging.Xunit;

namespace Kaponata.Operator.Tests
{
    /// <summary>
    /// A <see cref="LoggingConfig"/> whih uses the <see cref="LogFormatter"/>.
    /// </summary>
    public class LogConfig : LoggingConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogConfig"/> class.
        /// </summary>
        public LogConfig()
        {
            this.Formatter = new LogFormatter();
        }

        /// <summary>
        /// Gets the default logging configuration.
        /// </summary>
        public static LoggingConfig Default { get; } = new LogConfig();
    }
}
