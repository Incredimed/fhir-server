﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Azure.Documents;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage.Versioning
{
    public interface IUpdateOperation
    {
        void Apply(Document resource);
    }
}
