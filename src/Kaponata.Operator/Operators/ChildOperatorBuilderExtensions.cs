// <copyright file="ChildOperatorBuilderExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Extensions for the <see cref="ChildOperatorBuilder{TParent, TChild}"/> class.
    /// </summary>
    public static class ChildOperatorBuilderExtensions
    {
        /// <summary>
        /// Adds a feedback loop which initializes a new Appium session to the operator.
        /// </summary>
        /// <param name="builder">
        /// The operator builder.
        /// </param>
        /// <param name="appiumPort">
        /// The port at which the Appium server is listening.
        /// </param>
        /// <returns>
        /// An operator builder which can be used to further configure the operator.
        /// </returns>
        public static ChildOperatorBuilder<WebDriverSession, V1Pod> CreatesSession(
            this ChildOperatorBuilder<WebDriverSession, V1Pod> builder,
            int appiumPort)
        {
            return builder.PostsFeedback(
                async (context, cancellationToken) =>
                {
                    var kubernetes = context.Kubernetes;
                    var logger = context.Logger;
                    var session = context.Parent;
                    var pod = context.Child;

                    JsonPatchDocument<WebDriverSession> patch;

                    if (session?.Spec?.Capabilities == null)
                    {
                        // This is an invalid session; we need at least desired capabilities.
                        logger.LogWarning("Session {session} is missing desired capabilities.", session?.Metadata?.Name);
                        patch = null;
                    }
                    else if (session.Status?.SessionId != null)
                    {
                        // Do nothing if the session already exists.
                        logger.LogDebug("Session {session} already has a session ID.", session?.Metadata?.Name);
                        patch = null;
                    }
                    else if (pod?.Status?.Phase != "Running" || !pod.Status.ContainerStatuses.All(c => c.Ready))
                    {
                        // Do nothing if the pod is not yet ready
                        logger.LogInformation("Not creating a session for session {session} because pod {pod} is not ready yet.", session?.Metadata?.Name, pod?.Metadata?.Name);
                        patch = null;
                    }
                    else
                    {
                        var requestedCapabilities = JsonConvert.DeserializeObject(context.Parent.Spec.Capabilities);
                        var request = JsonConvert.SerializeObject(
                            new
                            {
                                capabilities = requestedCapabilities,
                            });

                        var content = new StringContent(request, Encoding.UTF8, "application/json");

                        using (var httpClient = kubernetes.CreatePodHttpClient(context.Child, appiumPort))
                        using (var remoteResult = await httpClient.PostAsync("wd/hub/session/", content, cancellationToken).ConfigureAwait(false))
                        {
                            var sessionJson = await remoteResult.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                            var sessionObject = JObject.Parse(sessionJson);
                            var sessionValue = (JObject)sessionObject.GetValue("value");

                            patch = new JsonPatchDocument<WebDriverSession>();

                            if (context.Parent.Status == null)
                            {
                                patch.Add(s => s.Status, new WebDriverSessionStatus());
                            }

                            // Check whether we should store this as a Kubernetes object.
                            if (sessionValue.TryGetValue("sessionId", out var sessionId))
                            {
                                patch.Add(s => s.Status.SessionId, sessionId.Value<string>());
                                patch.Add(s => s.Status.SessionReady, true);
                                patch.Add(s => s.Status.SessionPort, appiumPort);
                            }

                            if (sessionValue.TryGetValue("capabilities", out var capabilities))
                            {
                                patch.Add(s => s.Status.Capabilities, capabilities.ToString(Formatting.None));
                            }

                            if (sessionValue.TryGetValue("error", out var error))
                            {
                                patch.Add(s => s.Status.Error, error.Value<string>());
                            }

                            if (sessionValue.TryGetValue("message", out var message))
                            {
                                patch.Add(s => s.Status.Message, message.Value<string>());
                            }

                            if (sessionValue.TryGetValue("stacktrace", out var stackTrace))
                            {
                                patch.Add(s => s.Status.StackTrace, stackTrace.Value<string>());
                            }

                            if (sessionValue.TryGetValue("data", out var data))
                            {
                                patch.Add(s => s.Status.Data, data.ToString(Formatting.None));
                            }

                            return patch;
                        }
                    }

                    return null;
                });
        }
    }
}
