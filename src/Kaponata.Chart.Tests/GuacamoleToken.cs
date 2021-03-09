// <copyright file="GuacamoleToken.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Kaponata.Chart.Tests
{
    /// <summary>
    /// A token which can be used to authenticate with the Guacamole client.
    /// </summary>
    public class GuacamoleToken
    {
        /// <summary>
        /// Gets or sets the raw token.
        /// </summary>
        [JsonProperty("authToken")]
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the name of the user to which the token belongs.
        /// </summary>
        [JsonProperty("username")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the data source which was used to generate the token.
        /// </summary>
        [JsonProperty("dataSource")]
        public string DataSource { get; set; }

        /// <summary>
        /// Gets or sets a list of all available data sources.
        /// </summary>
        [JsonProperty("availableDataSources")]
        public List<string> AvailableDataSources { get; set; }
    }
}
