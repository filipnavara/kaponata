// <copyright file="LabelSelectorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Operator.Kubernetes;
using System;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes
{
    /// <summary>
    /// Tests the <see cref="LabelSelector"/> class.
    /// </summary>
    public class LabelSelectorTests
    {
        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns <see langword="null"/> when <see langword="null"/> is passed.
        /// </summary>
        [Fact]
        public void Create_Null_ReturnsNull()
        {
            Assert.Null(LabelSelector.Create<V1Pod>(null));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns <see langword="null"/> when an expression is passed when
        /// always returns <see langword="true"/>.
        /// </summary>
        [Fact]
        public void Create_AlwaysTrue_ReturnsNull()
        {
            Assert.Null(LabelSelector.Create<V1Pod>(s => true));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns <see langword="null"/> when an expression is passed when
        /// always returns <see langword="true"/>.
        /// </summary>
        [Fact]
        public void Create_AlwaysFalse_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LabelSelector.Create<V1Pod>(s => false));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns <see langword="null"/> when an expression is passed when
        /// always returns <see langword="true"/>.
        /// </summary>
        [Fact]
        public void Create_Complex_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LabelSelector.Create<V1Pod>(s => s.HasFinalizer("test")));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns <see langword="null"/> when no label is embedded in the expression.
        /// </summary>
        [Fact]
        public void Create_NoLabel_ReturnsNull()
        {
            Assert.Null(LabelSelector.Create<V1Pod>(p => p.Spec.Subdomain == "fake"));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns a correct label selector when a single label is present.
        /// </summary>
        [Fact]
        public void Create_SingleLabel_Works()
        {
            Assert.Equal("kubernetes.io/os=android", LabelSelector.Create<V1Pod>(p => p.Metadata.Labels[Annotations.Os] == Annotations.OperatingSystem.Android));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns a correct label selector when a two labels are present.
        /// </summary>
        [Fact]
        public void Create_TwoLabels_Works()
        {
            Assert.Equal("kubernetes.io/os=android,kubernetes.io/arch=arm64", LabelSelector.Create<V1Pod>(
                p => p.Metadata.Labels[Annotations.Os] == Annotations.OperatingSystem.Android
                && p.Metadata.Labels[Annotations.Arch] == Annotations.Architecture.Arm64));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns a correct label selector when a three labels are present.
        /// </summary>
        [Fact]
        public void Create_ThreeLabels_Works()
        {
            Assert.Equal("kubernetes.io/os=android,kubernetes.io/arch=arm64,app.kubernetes.io/managed-by=my-operator", LabelSelector.Create<V1Pod>(
                p => p.Metadata.Labels[Annotations.Os] == Annotations.OperatingSystem.Android
                && p.Metadata.Labels[Annotations.Arch] == Annotations.Architecture.Arm64
                && p.Metadata.Labels[Annotations.ManagedBy] == "my-operator"));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> throws an exception when the label value is not a constant.
        /// </summary>
        [Fact]
        public void Create_ValueNotConstant_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => LabelSelector.Create<V1Pod>(
                    p => p.Metadata.Labels[Annotations.Os] == p.ToString()));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns <see langword="null"/> when an incorrect method is being called
        /// on the Labels property.
        /// </summary>
        [Fact]
        public void Create_MethodCallOnWrongType_ReturnsNull()
        {
            Assert.Null(LabelSelector.Create<V1Pod>(p => p.Metadata.Labels.ToString() == "a"));
        }

        /// <summary>
        /// <see cref="LabelSelector.Create"/> returns <see langword="null"/> when an dictionary indexer is being called
        /// on the Labels property.
        /// </summary>
        [Fact]
        public void Create_MethodCallOnWrongProperty_ReturnsNull()
        {
            Assert.Null(LabelSelector.Create<V1Pod>(p => p.Spec.NodeSelector["a"] == "a"));
        }

        /// <summary>
        /// Calling <see cref="LabelSelector.Create"/> on a pod object which is not the parameter returns
        /// <see langword="null"/>.
        /// </summary>
        [Fact]
        public void Create_LabelOnNonParam_ReturnsNull()
        {
            Assert.Null(LabelSelector.Create<V1Pod>(p => new V1Pod().Metadata.Labels["a"] == "b"));
        }

        /// <summary>
        /// Calling <see cref="LabelSelector.Create"/> which reads a wrong property returns
        /// <see langword="null"/>.
        /// </summary>
        [Fact]
        public void Create_MethodCallOnWrongProperty2_ReturnsNull()
        {
            Assert.Null(LabelSelector.Create<V1Pod>(p => p.Metadata.ManagedFields.ToString() == "b"));
        }

        /// <summary>
        /// Calling <see cref="LabelSelector.Create"/> which reads a label using a non-constant
        /// expression returns <see langword="null"/>.
        /// </summary>
        [Fact]
        public void Create_LabelNameNotConstant_ReturnsNull()
        {
            Assert.Null(LabelSelector.Create<V1Pod>(p => p.Metadata.Labels["a".ToString()] == "b"));
        }
    }
}