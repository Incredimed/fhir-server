// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Extensions.DependencyInjection;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage
{
    public class MongoDbDistributedLockFactory : IMongoDbDistributedLockFactory
    {
        private readonly Func<IScoped<IDocumentClient>> _documentClientFactory;
        private readonly ILogger<MongoDbDistributedLock> _logger;

        public MongoDbDistributedLockFactory(Func<IScoped<IDocumentClient>> documentClientFactory, ILogger<MongoDbDistributedLock> logger)
        {
            EnsureArg.IsNotNull(documentClientFactory, nameof(documentClientFactory));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _documentClientFactory = documentClientFactory;
            _logger = logger;
        }

        public IMongoDbDistributedLock Create(Uri collectionUri, string lockId)
        {
            EnsureArg.IsNotNull(collectionUri, nameof(collectionUri));
            EnsureArg.IsNotNullOrEmpty(lockId, nameof(lockId));

            return new MongoDbDistributedLock(_documentClientFactory, collectionUri, lockId, _logger);
        }

        public IMongoDbDistributedLock Create(IDocumentClient client, Uri collectionUri, string lockId)
        {
            EnsureArg.IsNotNull(collectionUri, nameof(collectionUri));
            EnsureArg.IsNotNull(collectionUri, nameof(collectionUri));
            EnsureArg.IsNotNullOrEmpty(lockId, nameof(lockId));

            return new MongoDbDistributedLock(() => new NonDisposingScope(client), collectionUri, lockId, _logger);
        }
    }
}
