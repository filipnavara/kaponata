// <copyright file="Annotations.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes.Models;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Defines annotations commonly used by Kubernetes.
    /// </summary>
    public class Annotations
    {
        /// <summary>
        /// Applied to <see cref="V1Secret"/> objects, to indicate which aspect of a developer
        /// profile the secret represents. Possible values are <see cref="DeveloperIdentity"/>
        /// or <see cref="ProvisioningProfile"/>.
        /// </summary>
        public const string DeveloperProfileComponent = "kaponata.io/developerProfile";

        /// <summary>
        /// The secret contains a developer identity (an X.509 certificate).
        /// </summary>
        /// <seealso cref="DeveloperProfileComponent"/>.
        public const string DeveloperIdentity = "identity";

        /// <summary>
        /// The secret contains a provisioning profile.
        /// </summary>
        /// <seealso cref="DeveloperProfileComponent"/>.
        public const string ProvisioningProfile = "profile";

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
        /// The name of the automation provider used for a WebDriver session.
        /// </summary>
        public const string AutomationName = "kaponata.io/automation-name";

        /// <summary>
        /// The name of the <see cref="WebDriverSession"/> object which owns
        /// this object.
        /// </summary>
        public const string SessionName = "kaponata.io/session-name";

        /// <summary>
        /// A rule which is used to rewrite the target URL of an ingress rule.
        /// </summary>
        public const string RewriteTarget = "ingress.kubernetes.io/rewrite-target";

        /// <summary>
        /// Specifies which ingress class is to be used for the ingress. Used to force
        /// Kubernetes to use a specific reverse proxy (e.g. Traefik).
        /// </summary>
        public const string IngressClass = "kubernetes.io/ingress.class";

        /// <summary>
        /// Adds additional request modifiers. Can be used to configure URL rewriting.
        /// </summary>
        public const string RequestModifier = "traefik.ingress.kubernetes.io/request-modifier";

        /// <summary>
        /// Enumerates all automation names available.
        /// </summary>
        public class AutomationNames
        {
            /// <summary>
            /// The fake automation provider, implemented by the appium-fake-driver driver.
            /// </summary>
            public const string Fake = "fake";

            /// <summary>
            /// The UI Automator 2 provider enables test automation of Android devices.
            /// </summary>
            public const string UIAutomator2 = "uiautomator2";
        }

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
            /// The Linux operating system.
            /// </summary>
            public const string Linux = "linux";

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
