// <copyright file="ChildOperatorConfiguration.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Operator.Kubernetes;
using System;
using System.Collections.Generic;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Represents the configuration of a <see cref="ChildOperator{TParent, TChild}"/> instance.
    /// </summary>
    public class ChildOperatorConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildOperatorConfiguration"/> class.
        /// </summary>
        /// <param name="operatorName">
        /// The name of the operator.
        /// </param>
        public ChildOperatorConfiguration(string operatorName)
        {
            this.OperatorName = operatorName ?? throw new ArgumentNullException(nameof(operatorName));
            this.ChildLabels.Add(Annotations.ManagedBy, this.OperatorName);
        }

        /// <summary>
        /// Gets or sets the name of the namespace in which the operator observes object.
        /// </summary>
        public string Namespace { get; set; } = "default";

        /// <summary>
        /// Gets the name of the operator.
        /// </summary>
        public string OperatorName { get; }

        /// <summary>
        /// Gets or sets the label selector used to find to parents which are considered for reconciliation.
        /// </summary>
        public string ParentLabelSelector { get; set; }

        /// <summary>
        /// Gets the labels appleid to children created by the operator.
        /// </summary>
        public Dictionary<string, string> ChildLabels
        { get; } = new Dictionary<string, string>();
    }
}
