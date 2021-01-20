// <copyright file="KubernetesClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Net.Http;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// The <see cref="KubernetesClient"/> class extends the <see cref="k8s.Kubernetes"/> class
    /// and provides additional functionality.
    /// </summary>
    public partial class KubernetesClient : k8s.Kubernetes, IKubernetesClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesClient"/> class.
        /// </summary>
        /// <param name="handler">
        /// The <see cref="HttpMessageHandler"/> which is used to send HTTP requests.
        /// </param>
        public KubernetesClient(HttpMessageHandler handler)
        {
            this.HttpClient = new HttpClient(handler);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesClient"/> class.
        /// </summary>
        /// <param name="config">
        /// The Kubernetes client configuration.
        /// </param>
        public KubernetesClient(k8s.KubernetesClientConfiguration config)
            : base(config)
        {
            this.FirstMessageHandler = this.HttpClientHandler = CreateRootHandler();
            this.FirstMessageHandler = new WatchHandler(this.HttpClientHandler);

            this.HttpClient = new HttpClient(this.FirstMessageHandler, false);

            if (this.BaseUri.Scheme == "https")
            {
                if (config.SkipTlsVerify)
                {
                    this.HttpClientHandler.ServerCertificateCustomValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) => true;
                }
                else
                {
                    if (config.SslCaCerts == null)
                    {
                        throw new k8s.KubernetesException("A CA must be set when SkipTlsVerify === false");
                    }

                    this.HttpClientHandler.ServerCertificateCustomValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            return CertificateValidationCallBack(
                                sender,
                                config.SslCaCerts,
                                certificate,
                                chain,
                                sslPolicyErrors);
                        };
                }
            }

            if (this.BaseUri.Host == "0.0.0.0")
            {
                var builder = new UriBuilder(this.BaseUri);
                builder.Host = "127.0.0.1";
                this.BaseUri = builder.Uri;
            }

            // set credentails for the kubernetes client
            CreateCredentials(config);
            config.AddCertificates(this.HttpClientHandler);
        }
    }
}
