// <copyright file="Feedback.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Represents feedback of an operator to the state of a parent child pair.
    /// </summary>
    /// <typeparam name="TParent">
    /// The type of the parent object.
    /// </typeparam>
    /// <typeparam name="TChild">
    /// The type of the child object.
    /// </typeparam>
    public class Feedback<TParent, TChild>
        where TParent : class, IKubernetesObject<V1ObjectMeta>, new()
        where TChild : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        /// <summary>
        /// Gets or sets the feedback to apply to the parent object.
        /// </summary>
        public JsonPatchDocument<TParent> ParentFeedback { get; set; }

        /// <summary>
        /// Gets or sets the feedback to apply to the child object.
        /// </summary>
        public JsonPatchDocument<TChild> ChildFeedback { get; set; }
    }
}
