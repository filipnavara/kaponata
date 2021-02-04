// <copyright file="FeedbackLoop.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// A common delegate for all feedback loops. Decides whether the operator should provide feedback to the parent
    /// (by altering its state) and, if necessary, creates a patch document which represents the feedback.
    /// </summary>
    /// <param name="context">
    /// A <see cref="ChildOperatorContext{TParent, TChild}"/> object which represents the parent and child objects being
    /// evaluated.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
    /// </param>
    /// <typeparam name="TParent">
    /// The type of object to watch for (such as <see cref="V1Pod"/>).
    /// </typeparam>
    /// <typeparam name="TChild">
    /// The type of object to create (such as <see cref="V1Service"/>).
    /// </typeparam>
    /// <returns>
    /// A <see cref="Task"/> which represents the asynchronous operation, and returns a <see cref="JsonPatchDocument{TModel}"/>
    /// which describes the feedback if the child provides feedback to the parent (and the parent state should be alterated);
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public delegate Task<JsonPatchDocument<TParent>> FeedbackLoop<TParent, TChild>(
        ChildOperatorContext<TParent, TChild> context,
        CancellationToken cancellationToken)
        where TParent : class, IKubernetesObject<V1ObjectMeta>, new()
        where TChild : class, IKubernetesObject<V1ObjectMeta>, new();
}
