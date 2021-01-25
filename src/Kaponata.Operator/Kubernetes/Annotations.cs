// <copyright file="Annotations.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Defines annotations commonly used by Kubernetes.
    /// </summary>
    public class Annotations
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        /// <seealso href="https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/"/>
        public const string Name = "app.kubernetes.io/name";

        /// <summary>
        /// A unique name identifying the instance of an application.
        /// </summary>
        /// <seealso href="https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/"/>
        public const string Instance = "app.kubernetes.io/instance";

        /// <summary>
        /// The current version of the application (e.g., a semantic version, revision hash, etc.).
        /// </summary>
        /// <seealso href="https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/"/>
        public const string Version = "app.kubernetes.io/version";

        /// <summary>
        /// The component within the architecture.
        /// </summary>
        /// <seealso href="https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/"/>
        public const string Component = "app.kubernetes.io/component";

        /// <summary>
        /// The name of a higher level application this one is part of.
        /// </summary>
        /// <seealso href="https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/"/>
        public const string PartOf = "app.kubernetes.io/part-of";

        /// <summary>
        /// The tool being used to manage the operation of an application.
        /// </summary>
        /// <seealso href="https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/"/>
        public const string ManagedBy = "app.kubernetes.io/managed-by";

        /// <summary>
        /// The architecture of the Kubernetes node or mobile device.
        /// </summary>
        /// <seealso href="https://v1-17.docs.kubernetes.io/docs/reference/kubernetes-api/labels-annotations-taints/#kubernetes-io-arch"/>
        public const string Arch = "kubernetes.io/arch";

        /// <summary>
        /// The operating system of the Kubernetes node or mobile device.
        /// </summary>
        /// <seealso href="https://v1-17.docs.kubernetes.io/docs/reference/kubernetes-api/labels-annotations-taints/#kubernetes-io-os"/>
        public const string Os = "kubernetes.io/os";

        /// <summary>
        /// Enumerates the values of well-known computer architectures, as used by the <see cref="Arch"/> annotation and the
        /// <c>GOARCH</c> environment variable.
        /// </summary>
        /// <seealso href="https://github.com/golang/go/blob/release-branch.go1.15/src/cmd/dist/build.go#L61"/>
        public class Architecture
        {
            /// <summary>
            /// The AMD64 processor architecture.
            /// </summary>
            public const string Amd64 = "amd64";

            /// <summary>
            /// The 64-bit ARM processor architecture.
            /// </summary>
            public const string Arm64 = "arm64";
        }

        /// <summary>
        /// Enumerates values of well-known operating systems, as used by the <see cref="Os"/> annotation and the
        /// <c>GOOS</c> environment variable.
        /// </summary>
        /// <seealso href="https://github.com/golang/go/blob/release-branch.go1.15/src/cmd/dist/build.go#L79"/>
        public class OperatingSystem
        {
            /// <summary>
            /// The Google Android operating system.
            /// </summary>
            public const string Android = "android";

            /// <summary>
            /// The Apple iOS operating system.
            /// </summary>
            /// <remarks>
            /// This value was added recently, and is currently only available on Go master (https://github.com/golang/go/commit/431d58da69e8c36d654876e7808f971c5667649c).
            /// </remarks>
            public const string iOS = "ios";
        }
    }
}
