// <copyright file="MobileDevice.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes;
using Newtonsoft.Json;

namespace Kaponata.Kubernetes.Models
{
    /// <summary>
    /// A <see cref="MobileDevice"/> represents a physical iOS or Android devices connected to one of the nodes
    /// in your Kubernetes cluster, an emulator running inside your Kubernets cluster, or a
    /// cloud device which is remotely connected to your cluster.
    /// The name of the <see cref="MobileDevice"/> object is the serial number or UDID of the device.
    /// The <see cref="Annotations.Os"/> an <see cref="Annotations.Arch"/> labels contain the operating system (for example,
    /// <see cref="Annotations.OperatingSystem.iOS"/> or <see cref="Annotations.OperatingSystem.Android"/>) running
    /// on the device and the architecture(for example, <see cref="Annotations.Architecture.Arm64"/> or
    /// <see cref="Annotations.Architecture.Amd64"/>) of the device.
    /// </summary>
    public class MobileDevice : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>, ISpec<MobileDeviceSpec>
    {
        /// <summary>
        /// The API version used by Kubernetes to identify the object.
        /// </summary>
        public const string KubeApiVersion = KubeGroup + "/" + KubeVersion;

        /// <summary>
        /// The Kind used by Kubernetes to identify the object.
        /// </summary>
        public const string KubeKind = "MobileDevice";

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
        public const string KubePlural = "mobiledevices";

        /// <summary>
        /// Gets <see cref="KindMetadata"/> which describes the <see cref="MobileDevice"/> object type.
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
        public MobileDeviceSpec Spec { get; set; }

        /// <summary>
        /// Gets or sets an object which describes the status of the mobile device.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public MobileDeviceStatus Status { get; set; }
    }
}
