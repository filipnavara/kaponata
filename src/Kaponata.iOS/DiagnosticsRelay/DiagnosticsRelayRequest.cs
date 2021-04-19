// <copyright file="DiagnosticsRelayRequest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.PropertyLists;

namespace Kaponata.iOS.DiagnosticsRelay
{
    /// <summary>
    /// Represents a request sent to the <see cref="DiagnosticsRelayClient"/> service.
    /// </summary>
    public class DiagnosticsRelayRequest : IPropertyList
    {
        /// <summary>
        /// Gets or sets the type of this request. Known requests include <c>Restart</c>, <c>Shutdown</c>,
        /// <c>Goodbye</c> or <c>IORegistry</c>.
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// Gets or sets, when <see cref="Request"/> is <c>IORegistry</c>, the name of the registry
        /// entry being queried.
        /// </summary>
        public string EntryName { get; set; }

        /// <summary>
        /// Gets or sets, when <see cref="Request"/> is <c>IORegistry</c>, the class of the registry
        /// entry being queried.
        /// </summary>
        public string EntryClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to wait for the client to disconnect before
        /// executing the request, or not.
        /// </summary>
        public bool? WaitForDisconnect { get; set; }

        /// <inheritdoc/>
        public NSDictionary ToDictionary()
        {
            var dict = new NSDictionary();
            dict.Add(nameof(this.Request), this.Request);
            dict.AddWhenNotNull(nameof(this.EntryName), this.EntryName);
            dict.AddWhenNotNull(nameof(this.EntryClass), this.EntryClass);
            dict.AddWhenNotNull(nameof(this.WaitForDisconnect), this.WaitForDisconnect);
            return dict;
        }
    }
}
