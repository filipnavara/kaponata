// <copyright file="WebDriverSession.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes;
using Newtonsoft.Json;

namespace Kaponata.Kubernetes.Models
{
    /// <summary>
    /// A WebDriverSession object represents a WebDriver session which is currently running on
    /// a Kaponata device.It is typically created by a request to the Kaponata API server. An
    /// operator then picks up the session request and setting up the infrastructure to host
    /// the session.
    /// </summary>
    public class WebDriverSession : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>, ISpec<WebDriverSessionSpec>
    {
        /// <summary>
        /// The API version used by Kubernetes to identify the object.
        /// </summary>
        public const string KubeApiVersion = KubeGroup + "/" + KubeVersion;

        /// <summary>
        /// The Kind used by Kubernetes to identify the object.
        /// </summary>
        public const string KubeKind = "WebDriverSession";

        /// <summary>
        /// The Group used by Kubernetes to identify the object.
        /// </summary>
        public const string KubeGroup = "kaponata.io";

        /// <summary>
        /// The Version used by Kubernetes to identify the object.
        /// </summary>
        public const string KubeVersion = "v1alpha1";

        /// <summary>
        /// The plural name of the object type, used by Kubernetes to identify the object.
        /// </summary>
        public const string KubePlural = "webdriversessions";

        /// <summary>
        /// Gets <see cref="KindMetadata"/> which describes the <see cref="WebDriverSession"/> object type.
        /// </summary>
        public static KindMetadata KubeMetadata { get; } = new KindMetadata(KubeGroup, KubeVersion, KubePlural);

        /// <summary>
        /// Gets or sets the API version, which defines the versioned schema of this representation of
        /// an object. Servers should convert recognized schemas to the latest internal value,
        /// and may reject unrecognized values.
        /// </summary>
        /// <seealso href="https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources" />
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; } = KubeApiVersion;

        /// <summary>
        /// Gets or sets the object kind, which is a string value representing the REST resource this object
        /// represents. Servers may infer this from the endpoint the client submits requests
        /// Cannot be updated. In CamelCase.
        /// </summary>
        /// <seealso href="https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds"  />
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; } = KubeKind;

        /// <summary>
        /// Gets or sets standard object's metadata.
        /// </summary>
        /// <seealso href="https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata" />
        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "spec")]
        public WebDriverSessionSpec Spec { get; set; }

        /// <summary>
        /// Gets or sets an object which describes the status of the WebDriver session.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public WebDriverSessionStatus Status { get; set; }
    }
}
