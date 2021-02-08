// <copyright file="FieldSelectorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using System;
using Xunit;

namespace Kaponata.Kubernetes.Tests
{
    /// <summary>
    /// Tests the <see cref="FieldSelector"/> class.
    /// </summary>
    public class FieldSelectorTests
    {
        /// <summary>
        /// <see cref="FieldSelector.Create"/> returns <see langword="null"/> when passed <see langword="null"/>.
        /// </summary>
        [Fact]
        public void Create_Null_ReturnsNull()
        {
            Assert.Null(FieldSelector.Create<V1Pod>(null));
        }

        /// <summary>
        /// <see cref="FieldSelector.Create"/> returns <see langword="null"/> when passed a constant <see langword="true"/>
        /// expression (i.e. all objects match).
        /// </summary>
        [Fact]
        public void Create_True_ReturnsNull()
        {
            Assert.Null(FieldSelector.Create<V1Pod>(p => true));
        }

        /// <summary>
        /// <see cref="FieldSelector.Create"/> returns <see langword="null"/> when passed a constant <see langword="true"/>
        /// expression (i.e. all objects match).
        /// </summary>
        [Fact]
        public void Create_Complex_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FieldSelector.Create<V1Pod>(p => p.HasFinalizer("test")));
        }

        /// <summary>
        /// <see cref="FieldSelector.Create"/> returns <see langword="null"/> when passed a constant <see langword="true"/>
        /// expression (i.e. all objects match).
        /// </summary>
        [Fact]
        public void Create_Complex2_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FieldSelector.Create<V1Pod>(p => p.HasFinalizer("test") == p.HasFinalizer("foo")));
        }

        /// <summary>
        /// <see cref="FieldSelector.Create"/> works when passed a single value.
        /// </summary>
        [Fact]
        public void Create_SingleField_Works()
        {
            Assert.Equal(".spec.serviceAccountName=fake", FieldSelector.Create<V1Pod>(p => p.Spec.ServiceAccountName == "fake"));
        }

        /// <summary>
        /// <see cref="FieldSelector.Create"/> works when passed a single value.
        /// </summary>
        [Fact]
        public void Create_TwoFields_Or_Fails()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FieldSelector.Create<V1Pod>(p => p.Spec.ServiceAccountName == "fake" || p.Spec.Subdomain == null));
        }

        /// <summary>
        /// <see cref="FieldSelector.Create"/> works when passed a two values.
        /// </summary>
        [Fact]
        public void Create_TwoFields_Works()
        {
            Assert.Equal(
                ".status.phase=Running,.metadata.name=my-pod",
                FieldSelector.Create<V1Pod>(
                    p => p.Status.Phase == "Running"
                    && p.Metadata.Name == "my-pod"));
        }
    }
}
