// <copyright file="ImageRegistryClientConfiguration.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// The configuration for a <see cref="ImageRegistryClientFactory"/>.
    /// </summary>
    public class ImageRegistryClientConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRegistryClientConfiguration"/> class.
        /// </summary>
        /// <param name="serviceName">
        /// The name of the service which hosts the image registry.
        /// </param>
        /// <param name="port">
        /// The port at which the service can be reached.
        /// </param>
        public ImageRegistryClientConfiguration(string serviceName, int port)
        {
            this.ServiceName = serviceName;
            this.Port = port;
        }

        /// <summary>
        /// Gets the name of the service which hosts the image registry.
        /// </summary>
        public string ServiceName { get; private set; }

        /// <summary>
        /// Gets the port at which the service can be reached.
        /// </summary>
        public int Port { get; private set; }
    }
}
