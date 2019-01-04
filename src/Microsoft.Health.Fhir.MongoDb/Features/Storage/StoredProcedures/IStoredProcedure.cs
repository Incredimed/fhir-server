﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Azure.Documents;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage.StoredProcedures
{
    internal interface IStoredProcedure
    {
        string FullName { get; }

        Uri GetUri(Uri collection);

        StoredProcedure AsStoredProcedure();
    }
}
