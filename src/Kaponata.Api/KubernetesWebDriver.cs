// <copyright file="KubernetesWebDriver.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Api.WebDriver;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Api
{
    /// <summary>
    /// Implements the WebDriver API, using a Kubernetes cluster as a back-end.
    /// </summary>
    public class KubernetesWebDriver
    {
        private readonly KubernetesClient kubernetes;
        private readonly NamespacedKubernetesClient<WebDriverSession> sessionClient;
        private readonly ILogger<KubernetesWebDriver> logger;

        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesWebDriver"/> class.
        /// </summary>
        /// <param name="kubernetes">
        /// A <see cref="KubernetesClient"/> which can be used to connect to Kubernetes.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public KubernetesWebDriver(KubernetesClient kubernetes, ILogger<KubernetesWebDriver> logger)
        {
            this.kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.sessionClient = this.kubernetes.GetClient<WebDriverSession>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesWebDriver"/> class.
        /// </summary>
        /// <remarks>
        /// Intended for mocking/unit testing purposes only.
        /// </remarks>
#nullable disable
        protected KubernetesWebDriver()
#nullable enable
        {
        }

        /// <summary>
        /// Gets or sets the amount of time to wait for a session to be fully provisioned,
        /// before the operation times out.
        /// </summary>
        public TimeSpan CreationTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets the name of the automation provider for a given platform and automation name combination.
        /// </summary>
        /// <param name="platformName">
        /// The platform (e.g. Android, iOS,...) for which to provide automation.
        /// </param>
        /// <param name="automationName">
        /// The automation technology (e.g. uiautomator2, xcuitrunner) to use.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> which contains the name of the automation provider, or
        /// <see langword="null"/> if none could be found.
        /// </returns>
        public static string? GetProviderName(string platformName, string? automationName)
        {
            if (string.Equals("fake", platformName, StringComparison.OrdinalIgnoreCase))
            {
                return Annotations.AutomationNames.Fake;
            }
            else if (string.Equals("android", platformName, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals("uiautomator2", automationName, StringComparison.OrdinalIgnoreCase))
                {
                    return Annotations.AutomationNames.UIAutomator2;
                }
            }

            return null;
        }

        /// <summary>
        /// Asynchronously creates a new session.
        /// </summary>
        /// <param name="request">
        /// The capabilities requested by the client.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns a <see cref="WebDriverResponse"/>
        /// which represents the result of the operation.
        /// </returns>
        public virtual async Task<WebDriverResponse> CreateSessionAsync(NewSessionRequest request, CancellationToken cancellationToken)
        {
            if (request?.Capabilities?.AlwaysMatch == null)
            {
                return new WebDriverResponse(
                    new WebDriverError(
                        WebDriverErrorCode.SessionNotCreated,
                        "Required capabilities are missing"));
            }

            if (!request.Capabilities.AlwaysMatch.TryGetValue("platformName", out object? platformName)
                || !(platformName is string))
            {
                return new WebDriverResponse(
                    new WebDriverError(
                        WebDriverErrorCode.SessionNotCreated,
                        "The platformName capability is required and must be a string."));
            }

            request.Capabilities.AlwaysMatch.TryGetValue("appium:automationName", out object? automationName);

            if (automationName != null & !(automationName is string))
            {
                return new WebDriverResponse(
                    new WebDriverError(
                        WebDriverErrorCode.SessionNotCreated,
                        "The appium:automationName capability must be a string."));
            }

            var providerName = GetProviderName((string)platformName, automationName as string);

            if (providerName == null)
            {
                return new WebDriverResponse(
                    new WebDriverError(
                        WebDriverErrorCode.SessionNotCreated,
                        $"The platform '{platformName}' in combination with appium:automationName '{automationName}' is not supported"));
            }

            var session = await this.sessionClient.CreateAsync(
                new WebDriverSession()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        NamespaceProperty = "default",
                        GenerateName = $"{providerName}-",
                        Labels = new Dictionary<string, string>()
                        {
                            { Annotations.AutomationName, providerName },
                        },
                    },
                    Spec = new WebDriverSessionSpec()
                    {
                        Capabilities = JsonConvert.SerializeObject(request.Capabilities, this.serializerSettings),
                    },
                },
                cancellationToken).ConfigureAwait(false);

            // Wait for the session to:
            // - have a valid session ID (session creation was successful)
            // - has an error (session creation failed)
            // - has been deleted (somebody interfered)
            // - time out
            var watcher = this.sessionClient.WatchAsync(
                session,
                (eventType, updatedSession) =>
                {
                    switch (eventType)
                    {
                        case WatchEventType.Deleted:
                            return Task.FromResult(WatchResult.Stop);

                        case WatchEventType.Modified when SessionIsReadyOrHasFailed(updatedSession):
                            return Task.FromResult(WatchResult.Stop);

                        default:
                            return Task.FromResult(WatchResult.Continue);
                    }
                },
                cancellationToken);

            // Give the session one minute to be properly initialized.
            if (await Task.WhenAny(watcher, Task.Delay(this.CreationTimeout)) == watcher)
            {
                session = await this.sessionClient.TryReadAsync(session.Metadata.Name, cancellationToken).ConfigureAwait(false);

                if (SessionIsReady(session))
                {
                    return new WebDriverResponse(
                        new NewSessionResponse
                        {
                            SessionId = session.Metadata.Name,
                            Capabilities = JsonConvert.DeserializeObject<Dictionary<string, object>>(session.Status.Capabilities),
                        });
                }
                else if (session != null)
                {
                    await this.sessionClient.DeleteAsync(session, TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false);

                    return new WebDriverResponse(
                        new WebDriverError(WebDriverErrorCode.SessionNotCreated)
                        {
                            Data = session.Status.Data,
                            Message = session.Status.Message,
                            StackTrace = session.Status.StackTrace,
                        });
                }
                else
                {
                    return new WebDriverResponse(
                        new WebDriverError(WebDriverErrorCode.SessionNotCreated));
                }
            }
            else
            {
                return new WebDriverResponse(
                    new WebDriverError(WebDriverErrorCode.SessionNotCreated)
                    {
                        Message = "The session creation timed out.",
                    });
            }
        }

        /// <summary>
        /// Asynchronously deletes a session.
        /// </summary>
        /// <param name="sessionId">
        /// The ID of the session to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public virtual async Task<WebDriverResponse> DeleteSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
            // Find session by label
            var session = await this.sessionClient.TryReadAsync(
                sessionId,
                cancellationToken).ConfigureAwait(false);

            if (session == null)
            {
                return new WebDriverResponse(
                    new WebDriverError(WebDriverErrorCode.InvalidSessionId));
            }
            else
            {
                await this.sessionClient.DeleteAsync(
                    session,
                    new V1DeleteOptions() { PropagationPolicy = "Foreground" },
                    TimeSpan.FromMinutes(1),
                    cancellationToken).ConfigureAwait(false);

                return new WebDriverResponse();
            }
        }

        private static bool SessionIsReady([NotNullWhen(true)] WebDriverSession? session)
        {
            if (session?.Status == null)
            {
                return false;
            }

            var status = session.Status;

            return status.SessionReady && status.IngressReady && status.ServiceReady;
        }

        private static bool SessionIsReadyOrHasFailed(WebDriverSession session)
        {
            if (session?.Status == null)
            {
                return false;
            }

            var status = session.Status;

            if (status.Error != null)
            {
                return true;
            }

            if (status.SessionReady && status.IngressReady && status.ServiceReady)
            {
                return true;
            }

            return false;
        }
    }
}
