﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage.StoredProcedures.Upsert
{
    internal class UpsertWithHistory : StoredProcedureBase
    {
        public async Task<UpsertWithHistoryModel> Execute(IDocumentClient client, Uri collection, MongoResourceWrapper resource, string matchVersionId, bool allowCreate, bool keepHistory)
        {
            EnsureArg.IsNotNull(client, nameof(client));
            EnsureArg.IsNotNull(collection, nameof(collection));
            EnsureArg.IsNotNull(resource, nameof(resource));

            StoredProcedureResponse<UpsertWithHistoryModel> results =
                await ExecuteStoredProc<UpsertWithHistoryModel>(client, collection, resource.PartitionKey, resource, matchVersionId, allowCreate, keepHistory);

            return results.Response;
        }
    }
}
