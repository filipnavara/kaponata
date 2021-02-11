// <copyright file="FakeOperatorExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Provides extension methods for adding the fake operator to a <see cref="ServiceCollection"/>.
    /// </summary>
    public static class FakeOperatorExtensions
    {
        /// <summary>
        /// Adds the fake operators to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">
        /// The service collection to which to add the fake operators.
        /// </param>
        /// <returns>
        /// The service collection.
        /// </returns>
        public static IServiceCollection AddFakeOperators(this IServiceCollection services)
        {
            services.AddHostedService((serviceProvider) => FakeOperators.BuildPodOperator(serviceProvider).Build());
            services.AddHostedService((serviceProvider) => FakeOperators.BuildServiceOperator(serviceProvider).Build());
            services.AddHostedService((serviceProvider) => FakeOperators.BuildIngressOperator(serviceProvider).Build());

            return services;
        }
    }
}
