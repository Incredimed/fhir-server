// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Azure.Documents;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage
{
    public interface IMongoDbDistributedLockFactory
    {
        IMongoDbDistributedLock Create(Uri collectionUri, string lockId);

        IMongoDbDistributedLock Create(IDocumentClient client, Uri collectionUri, string lockId);
    }
}
