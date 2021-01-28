// <copyright file="WebDriverSessionList.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Kaponata.Operator.Models
{
    /// <summary>
    /// A list of <see cref="WebDriverSession"/> objects.
    /// </summary>
    public class WebDriverSessionList : IKubernetesObject<V1ListMeta>, IItems<WebDriverSession>
    {
        /// <summary>
        /// Gets or sets a value which defines the versioned schema of this
        /// representation of an object. Servers should convert recognized
        /// schemas to the latest internal value, and may reject unrecognized
        /// values.
        /// </summary>
        /// <seealso href="https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources"/>
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets list of WebDriver sessions.
        /// </summary>
        /// <seealso href="https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md"/>
        [JsonProperty(PropertyName = "items")]
        public IList<WebDriverSession> Items { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="string"/> value representing the REST resource
        /// this object represents. Servers may infer this from the endpoint
        /// the client submits requests to. Cannot be updated. In CamelCase.
        /// </summary>
        /// <seealso href="https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds"/>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets standard list metadata.
        /// </summary>
        /// <seealso href="https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds"/>
        [JsonProperty(PropertyName = "metadata")]
        public V1ListMeta Metadata { get; set; }
    }
}
