﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Health.Fhir.MongoDb.Configs;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage
{
    public interface IDocumentClientTestProvider
    {
        Task PerformTest(IDocumentClient documentClient, MongoDataStoreConfiguration configuration);
    }
}
