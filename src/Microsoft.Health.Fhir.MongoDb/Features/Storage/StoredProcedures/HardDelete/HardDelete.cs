﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Health.Fhir.Core.Features.Persistence;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage.StoredProcedures.HardDelete
{
    internal class HardDelete : StoredProcedureBase
    {
        public async Task<StoredProcedureResponse<IList<string>>> Execute(IDocumentClient client, Uri collection, ResourceKey key)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNull(collection, nameof(collection));
            EnsureArg.IsNotNull(key, nameof(key));

            return await ExecuteStoredProc<IList<string>>(client, collection, key.ToPartitionKey(), key.ResourceType, key.Id);
        }
    }
}
