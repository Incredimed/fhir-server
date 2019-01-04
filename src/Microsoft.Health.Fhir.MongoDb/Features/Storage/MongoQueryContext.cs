// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage
{
    /// <summary>
    /// Context used for executing a mongo query.
    /// </summary>
    public class MongoQueryContext : IMongoQueryContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryContext"/> class.
        /// </summary>
        /// <param name="collectionUri">The collection URI.</param>
        /// <param name="sqlQuerySpec">The SQL query.</param>
        /// <param name="feedOptions">The options.</param>
        public MongoQueryContext(Uri collectionUri, SqlQuerySpec sqlQuerySpec, FeedOptions feedOptions = null)
        {
            EnsureArg.IsNotNull(collectionUri, nameof(collectionUri));
            EnsureArg.IsNotNull(sqlQuerySpec, nameof(sqlQuerySpec));

            CollectionUri = collectionUri;
            SqlQuerySpec = sqlQuerySpec;
            FeedOptions = feedOptions;
        }

        /// <inheritdoc />
        public Uri CollectionUri { get; }

        /// <inheritdoc />
        public SqlQuerySpec SqlQuerySpec { get; }

        /// <inheritdoc />
        public FeedOptions FeedOptions { get; }
    }
}
