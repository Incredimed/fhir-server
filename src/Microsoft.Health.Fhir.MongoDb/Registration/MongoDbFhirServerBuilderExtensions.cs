// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.Core.Features.Health;
using Microsoft.Health.Fhir.Core.Features.Initialization;
using Microsoft.Health.Fhir.Core.Registration;
using Microsoft.Health.Fhir.MongoDb.Configs;
using Microsoft.Health.Fhir.MongoDb.Features.Health;
using Microsoft.Health.Fhir.MongoDb.Features.Search;
using Microsoft.Health.Fhir.MongoDb.Features.Search.Queries;
using Microsoft.Health.Fhir.MongoDb.Features.Storage;
using Microsoft.Health.Fhir.MongoDb.Features.Storage.Versioning;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDbFhirServerBuilderExtensions
    {
        /// <summary>
        /// Adds MongoDB as the data store for the FHIR server.
        /// Settings are read from the "MongoDb" configuration section and can optionally be overridden with the <paramref name="configureAction"/> delegate.
        /// </summary>
        /// <param name="fhirServerBuilder">The FHIR server builder.</param>
        /// <param name="configureAction">An optional delegate for overriding configuration properties.</param>
        /// <returns>The builder.</returns>
        public static IFhirServerBuilder AddMongoDb(this IFhirServerBuilder fhirServerBuilder, Action<MongoDataStoreConfiguration> configureAction = null)
        {
            EnsureArg.IsNotNull(fhirServerBuilder, nameof(fhirServerBuilder));

            return fhirServerBuilder
                .AddMongoDbPersistence(configureAction)
                .AddMongoDbSearch()
                .AddMongoDbHealthCheck();
        }

        private static IFhirServerBuilder AddMongoDbPersistence(this IFhirServerBuilder fhirServerBuilder, Action<MongoDataStoreConfiguration> configureAction)
        {
            IServiceCollection services = fhirServerBuilder.Services;

            services.Add(provider =>
                {
                    var config = new MongoDataStoreConfiguration();
                    provider.GetService<IConfiguration>().GetSection("MongoDb").Bind(config);
                    configureAction?.Invoke(config);

                    if (string.IsNullOrEmpty(config.Host))
                    {
                        config.Host = MongoDbLocalEmulator.Host;
                        config.Key = MongoDbLocalEmulator.Key;
                    }

                    return config;
                })
                .Singleton()
                .AsSelf();

            services.Add<MongoDataStore>()
                .Scoped()
                .AsSelf()
                .AsImplementedInterfaces();

            services.Add<DocumentClientProvider>()
                .Singleton()
                .AsSelf()
                .AsService<IStartable>() // so that it starts initializing ASAP
                .AsService<IRequireInitializationOnFirstRequest>(); // so that web requests block on its initialization.

            services.Add<DocumentClientReadWriteTestProvider>()
                .Singleton()
                .AsService<IDocumentClientTestProvider>();

            // Register IDocumentClient
            // We are intentionally not registering IDocumentClient directly, because
            // we want this codebase to support different configurations, where the
            // lifetime of the document clients can be managed outside of the IoC
            // container, which will automatically dispose it if exposed as a scoped
            // service or as transient but consumed from another scoped service.

            services.Add(sp => sp.GetService<DocumentClientProvider>().CreateDocumentClientScope())
                .Transient()
                .AsSelf()
                .AsFactory();

            services.Add<DocumentClientInitializer>()
                .Singleton()
                .AsService<IDocumentClientInitializer>();

            services.Add<MongoDocumentQueryFactory>()
                .Singleton()
                .AsService<IMongoDocumentQueryFactory>();

            services.Add<MongoDocumentQueryLogger>()
                .Singleton()
                .AsService<IMongoDocumentQueryLogger>();

            services.Add<CollectionUpgradeManager>()
                .Singleton()
                .AsService<IUpgradeManager>();

            services.TypesInSameAssemblyAs<ICollectionUpdater>()
                .AssignableTo<ICollectionUpdater>()
                .Singleton()
                .AsSelf()
                .AsService<ICollectionUpdater>();

            services.Add<MongoDbDistributedLockFactory>()
                .Singleton()
                .AsService<IMongoDbDistributedLockFactory>();

            services.Add<RetryExceptionPolicyFactory>()
                .Singleton()
                .AsSelf();

            return fhirServerBuilder;
        }

        private static IFhirServerBuilder AddMongoDbHealthCheck(this IFhirServerBuilder fhirServerBuilder)
        {
            // We can move to framework such as https://github.com/dotnet-architecture/HealthChecks
            // once they are released to do health check on multiple dependencies.
            fhirServerBuilder.Services.Add<MongoHealthCheck>()
                .Scoped()
                .AsSelf()
                .AsService<IHealthCheck>();

            return fhirServerBuilder;
        }

        private static IFhirServerBuilder AddMongoDbSearch(this IFhirServerBuilder fhirServerBuilder)
        {
            fhirServerBuilder.Services.Add<MongoSearchService>()
                .Scoped()
                .AsSelf()
                .AsImplementedInterfaces();

            fhirServerBuilder.Services.AddSingleton<IQueryBuilder, QueryBuilder>();

            return fhirServerBuilder;
        }
    }
}
