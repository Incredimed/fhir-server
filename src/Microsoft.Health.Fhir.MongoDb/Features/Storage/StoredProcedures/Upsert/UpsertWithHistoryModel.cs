﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Fhir.Core.Features.Persistence;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage.StoredProcedures.Upsert
{
    internal class UpsertWithHistoryModel
    {
        [JsonConstructor]
        protected UpsertWithHistoryModel()
        {
        }

        [JsonProperty("outcomeType")]
        public SaveOutcomeType OutcomeType { get; protected set; }

        [JsonProperty("wrapper")]
        public MongoResourceWrapper Wrapper { get; protected set; }
    }
}
