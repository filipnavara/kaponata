// <copyright file="StartServiceResponse.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;

namespace Kaponata.iOS.Lockdown
{
    /// <content>
    /// Methods for deserializing the <see cref="StartServiceResponse"/>.
    /// </content>
    public partial class StartServiceResponse
    {
        /// <inheritdoc/>
        public override void FromDictionary(NSDictionary data)
        {
            base.FromDictionary(data);

            this.EnableServiceSSL = data.GetNullableBoolean(nameof(this.EnableServiceSSL)) ?? false;

            this.Port = data.GetNullableInt32(nameof(this.Port)) ?? 0;
            this.Service = data.GetString(nameof(this.Service));
        }
    }
}
