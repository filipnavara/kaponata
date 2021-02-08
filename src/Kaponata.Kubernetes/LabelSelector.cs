// <copyright file="LabelSelector.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Supports converting lambda expressions to label selectors.
    /// </summary>
    /// <seealso href="https://kubernetes.io/docs/concepts/overview/working-with-objects/labels/"/>
    public static class LabelSelector
    {
        /// <summary>
        /// Converts a lambda expression to a label selector.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Kubernetes object to which the label selector applies.
        /// </typeparam>
        /// <param name="predicate">
        /// The label selector, as a lambda expression.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> which represents the label selector.
        /// </returns>
        public static string Create<T>(Expression<Func<T, bool>> predicate)
            where T : IKubernetesObject<V1ObjectMeta>
        {
            if (predicate == null)
            {
                return null;
            }

            return ToLabelSelector(predicate.Body);
        }

        // Operates on the root expression. We support either a binary Equal expression
        // (e.g. label = value) or an AndAlso expression. (label1 = value1 && label2 = value2)
        private static string ToLabelSelector(Expression expression)
        {
            switch (expression)
            {
                case ConstantExpression constantExpression when object.Equals(constantExpression.Value, true):
                    return null;

                case BinaryExpression binaryExpression when binaryExpression.NodeType == ExpressionType.Equal:
                    return ToSingleLabelSelector(binaryExpression);

                case BinaryExpression binaryExpression when binaryExpression.NodeType == ExpressionType.AndAlso:
                    var left = ToLabelSelector(binaryExpression.Left);
                    var right = ToLabelSelector(binaryExpression.Right);
                    return $"{left},{right}";
            }

            throw new ArgumentOutOfRangeException(nameof(expression), "Only binary expressions of type Equal and AndAlso are supported");
        }

        // Operates on a binary Equal expression (label = value). Extracts the name of the
        // label from the left part of the expression and the constant value from the
        // right part of the expression.
        private static string ToSingleLabelSelector(BinaryExpression binaryExpression)
        {
            Debug.Assert(binaryExpression.NodeType == ExpressionType.Equal, "Only equal expressions are supported");

            if (!IsLabelExpression(binaryExpression.Left, out var labelName))
            {
                return null;
            }

            if (!IsLabelValue(binaryExpression.Right, out var labelValue))
            {
                throw new ArgumentOutOfRangeException(nameof(binaryExpression));
            }

            return $"{labelName}={labelValue}";
        }

        // Extracts a label value from a constant string expression.
        private static bool IsLabelValue(Expression expression, out string labelValue)
        {
            labelValue = null;

            switch (expression)
            {
                case ConstantExpression constantExpression:
                    labelValue = constantExpression.Value as string;
                    return true;
            }

            return false;
        }

        // Extracts a label name from a method call expression. Labels are typically
        // assigned like this:
        // p.Metadata.Labels["name"] = "value",
        // and this method is concerned with p.Metadata.Labels["name"].
        //
        // This method makes sure the expression is indeed correct:
        // * The ["name"] part is syntactic sugar for get_Item("name"), so the top level
        //   expression should be a MethodCallExpression
        // * .Metadata.Labels are property accessors, and these are also verified.
        // * Finally, the root object,  'p' is a parameter.
        private static bool IsLabelExpression(Expression expression, out string labelName)
        {
            labelName = null;
            var methodCall = expression as MethodCallExpression;

            if (methodCall == null)
            {
                return false;
            }

            if (methodCall.Object.Type != typeof(IDictionary<string, string>))
            {
                return false;
            }

            if (methodCall.Method.Name != "get_Item")
            {
                return false;
            }

            if (!IsProperty(methodCall.Object, typeof(V1ObjectMeta), nameof(V1ObjectMeta.Labels), out var metadataCall))
            {
                return false;
            }

            if (!IsProperty(metadataCall, typeof(IKubernetesObject<V1ObjectMeta>), nameof(IKubernetesObject<V1ObjectMeta>.Metadata), out var root))
            {
                return false;
            }

            if (root.NodeType != ExpressionType.Parameter)
            {
                return false;
            }

            var labelExpression = methodCall.Arguments[0] as ConstantExpression;

            if (labelExpression == null)
            {
                return false;
            }

            labelName = labelExpression.Value as string;
            return true;
        }

        // Determines whether an expression is a property reference expression.
        private static bool IsProperty(Expression expression, Type type, string name, out Expression parent)
        {
            parent = null;
            var memberExpression = expression as MemberExpression;

            bool isProperty = memberExpression != null
                && memberExpression.Member.ReflectedType.IsAssignableTo(type)
                && memberExpression.Member.Name == name;

            if (isProperty)
            {
                parent = memberExpression.Expression;
            }

            return isProperty;
        }
    }
}
