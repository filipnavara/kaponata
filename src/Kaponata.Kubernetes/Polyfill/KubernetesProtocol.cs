// <copyright file="KubernetesProtocol.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace Kaponata.Kubernetes.Polyfill
{
    /// <summary>
    /// The <see cref="KubernetesProtocol"/> class extends the <see cref="k8s.Kubernetes"/> class
    /// and provides additional functionality.
    /// </summary>
    public partial class KubernetesProtocol : k8s.Kubernetes, IKubernetesProtocol
    {
        private readonly ILogger<KubernetesProtocol> logger;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesProtocol"/> class.
        /// </summary>
        /// <param name="handler">
        /// The <see cref="HttpMessageHandler"/> which is used to send HTTP requests.
        /// </param>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A logger factory which is used to create new logger objects.
        /// </param>
        public KubernetesProtocol(HttpMessageHandler handler, ILogger<KubernetesProtocol> logger, ILoggerFactory loggerFactory)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            this.HttpClient = new HttpClient(handler);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesProtocol"/> class.
        /// </summary>
        /// <param name="config">
        /// The Kubernetes client configuration.
        /// </param>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A logger factory which is used to create new logger objects.
        /// </param>
        public KubernetesProtocol(k8s.KubernetesClientConfiguration config, ILogger<KubernetesProtocol> logger, ILoggerFactory loggerFactory)
            : base(config)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            this.FirstMessageHandler = this.HttpClientHandler = CreateRootHandler();
            this.FirstMessageHandler = new CoreApiHandler(new WatchHandler(this.HttpClientHandler));

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

            // set credentails for the kubernetes client
            CreateCredentials(config);
            config.AddCertificates(this.HttpClientHandler);
        }
    }
}
