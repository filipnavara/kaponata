// <copyright file="FieldSelector.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Provides methods for creating field selectors from lambda expressions.
    /// </summary>
    public static class FieldSelector
    {
        /// <summary>
        /// The default resolver to use when converting .NET object property names to JSON object property names.
        /// </summary>
        private static readonly IContractResolver DefaultResolver = new CamelCasePropertyNamesContractResolver();

        /// <summary>
        /// Converts a labmda expression to a field selector.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Kubernetes object to which the field selector applies.
        /// </typeparam>
        /// <param name="predicate">
        /// The predicate which represents the field selector.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> which represents the field selector.
        /// </returns>
        public static string Create<T>(Expression<Func<T, bool>> predicate)
            where T : IKubernetesObject<V1ObjectMeta>
        {
            if (predicate == null)
            {
                return null;
            }

            return ToFieldSelector(predicate.Body);
        }

        // Converts an generic expression. This should be a binary expression; either Equal
        // (e.g. pod.Status.Phase == "Running") or AndAlso (pod.Status.Phase == "Running" && pod.Metadata.Name == "a")
        private static string ToFieldSelector(Expression expression)
        {
            switch (expression)
            {
                case ConstantExpression constantExpression when object.Equals(constantExpression.Value, true):
                    return null;

                case BinaryExpression binaryExpression:
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.Equal:
                            return ToSingleFieldSelector(binaryExpression);

                        case ExpressionType.AndAlso:
                            var left = ToFieldSelector(binaryExpression.Left);
                            var right = ToFieldSelector(binaryExpression.Right);
                            return $"{left},{right}";
                    }

                    break;
            }

            throw new ArgumentOutOfRangeException($"The {expression.NodeType} expressions are not supported");
        }

        // Converts a single binary expression of type Equal (e.g. pod.Status.Phase == "Running") to a
        // single field expression (pod.status.phase=Running).
        private static string ToSingleFieldSelector(BinaryExpression binaryExpression)
        {
            Debug.Assert(binaryExpression.NodeType == ExpressionType.Equal, "Only Equal expressions are supported");

            var valueExpression = binaryExpression.Right as ConstantExpression;

            if (valueExpression == null)
            {
                throw new ArgumentOutOfRangeException(nameof(binaryExpression), "Only Equal expressions which compare to constant values are supported");
            }

            var value = valueExpression.Value as string;

            var propertyExpression = binaryExpression.Left as MemberExpression;

            var path = GetPath(propertyExpression);
            return $"{path}={value}";
        }

        // Gets the path of an property. Recurses up the parent properties until the root object
        // (a parameter) is found.
        private static string GetPath(MemberExpression expression)
        {
            var name = GetJsonPropertyName(expression.Member.DeclaringType, expression.Member.Name);

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                return $".{name}";
            }
            else
            {
                var child = expression.Expression as MemberExpression;
                return $"{GetPath(child)}.{name}";
            }
        }

        // Gets the JSON property name for a .NET object property name.
        private static string GetJsonPropertyName(Type type, string propertyName)
        {
            var contract = DefaultResolver.ResolveContract(type) as JsonObjectContract;
            var property = contract.Properties.Single(p => p.UnderlyingName == propertyName);
            return property.PropertyName;
        }
    }
}
