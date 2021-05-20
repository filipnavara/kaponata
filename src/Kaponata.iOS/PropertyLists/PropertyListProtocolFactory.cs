// <copyright file="PropertyListProtocolFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using System.IO;

namespace Kaponata.iOS.PropertyLists
{
    /// <summary>
    /// Creates new instances of the <see cref="PropertyListProtocol"/> class.
    /// </summary>
    public class PropertyListProtocolFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyListProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="PropertyListProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="PropertyListProtocol"/> class.
        /// </returns>
        public virtual PropertyListProtocol Create(Stream stream, bool ownsStream, ILogger logger)
        {
            return new PropertyListProtocol(stream, ownsStream, logger);
        }
    }
}
