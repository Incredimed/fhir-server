// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage
{
    /// <summary>
    /// Context used for executing a mongo query.
    /// </summary>
    public interface IMongoQueryContext
    {
        /// <summary>
        /// Gets the collection URI.
        /// </summary>
        Uri CollectionUri { get; }

        /// <summary>
        /// Gets the SQL query.
        /// </summary>
        SqlQuerySpec SqlQuerySpec { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        FeedOptions FeedOptions { get; }
    }
}
