// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class NorthwindQueryInMemoryFixture : NorthwindQueryFixtureBase
    {
        private readonly TestLoggerFactory _testLoggerFactory = new TestLoggerFactory();

        public override NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll,
            bool enableFilters = false)
        {
            if (!IsSeeded)
            {
                using (var context = base.CreateContext(queryTrackingBehavior, enableFilters))
                {
                    NorthwindData.Seed(context);
                }

                IsSeeded = true;
            }

            return base.CreateContext(queryTrackingBehavior, enableFilters);
        }

        private bool IsSeeded { get; set; }

        public override DbContextOptions BuildOptions(IServiceCollection serviceCollection = null)
            => new DbContextOptionsBuilder()
                .UseInMemoryDatabase(nameof(NorthwindQueryInMemoryFixture))
                .UseInternalServiceProvider(
                    (serviceCollection ?? new ServiceCollection())
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(_testLoggerFactory)
                    .BuildServiceProvider())
                .Options;
    }
}
