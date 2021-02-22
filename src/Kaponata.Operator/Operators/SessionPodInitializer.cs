// <copyright file="SessionPodInitializer.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// A common delegate for any action which configures a pod related to a WebDriver session.
    /// </summary>
    /// <param name="context">
    /// A <see cref="ChildOperatorContext{TParent, TChild}"/> object which represents the parent and child objects being
    /// evaluated.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> which represents the asynchronous operation.
    /// </returns>
    public delegate Task SessionPodInitializer(
        ChildOperatorContext<WebDriverSession, V1Pod> context,
        CancellationToken cancellationToken);
}
