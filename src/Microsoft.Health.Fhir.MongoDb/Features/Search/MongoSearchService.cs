﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Health.Fhir.Core.Features.Persistence;
using Microsoft.Health.Fhir.Core.Features.Search;
using Microsoft.Health.Fhir.MongoDb.Features.Search.Queries;
using Microsoft.Health.Fhir.MongoDb.Features.Storage;
using Microsoft.Health.Fhir.MongoDb.Features.Storage.Continuation;

namespace Microsoft.Health.Fhir.MongoDb.Features.Search
{
    public class MongoSearchService : SearchService
    {
        private readonly MongoDataStore _mongoDataStore;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IContinuationTokenCache _continuationTokenCache;

        public MongoSearchService(
            ISearchOptionsFactory searchOptionsFactory,
            MongoDataStore mongoDataStore,
            IQueryBuilder queryBuilder,
            IBundleFactory bundleFactory,
            IContinuationTokenCache continuationTokenCache)
            : base(searchOptionsFactory, bundleFactory, mongoDataStore)
        {
            EnsureArg.IsNotNull(mongoDataStore, nameof(mongoDataStore));
            EnsureArg.IsNotNull(queryBuilder, nameof(queryBuilder));

            _mongoDataStore = mongoDataStore;
            _queryBuilder = queryBuilder;
            _continuationTokenCache = continuationTokenCache;
        }

        protected override async Task<SearchResult> SearchInternalAsync(
            SearchOptions searchOptions,
            CancellationToken cancellationToken)
        {
            return await ExecuteSearchAsync(
                _queryBuilder.BuildSqlQuerySpec(searchOptions),
                searchOptions,
                cancellationToken);
        }

        protected override async Task<SearchResult> SearchHistoryInternalAsync(
            SearchOptions searchOptions,
            CancellationToken cancellationToken)
        {
            return await ExecuteSearchAsync(
                _queryBuilder.GenerateHistorySql(searchOptions),
                searchOptions,
                cancellationToken);
        }

        private async Task<SearchResult> ExecuteSearchAsync(
            SqlQuerySpec sqlQuerySpec,
            SearchOptions searchOptions,
            CancellationToken cancellationToken)
        {
            string ct = null;

            if (!string.IsNullOrEmpty(searchOptions.ContinuationToken))
            {
                ct = await _continuationTokenCache.GetContinuationTokenAsync(searchOptions.ContinuationToken, cancellationToken);
            }

            var feedOptions = new FeedOptions
            {
                EnableCrossPartitionQuery = true,
                MaxItemCount = searchOptions.MaxItemCount,
                RequestContinuation = ct,
            };

            if (searchOptions.CountOnly)
            {
                IDocumentQuery<int> documentCountQuery = _mongoDataStore.CreateDocumentQuery<int>(sqlQuerySpec, feedOptions);

                using (documentCountQuery)
                {
                    return new SearchResult(Enumerable.Empty<ResourceWrapper>(), null)
                    {
                        TotalCount = (await documentCountQuery.ExecuteNextAsync<int>(cancellationToken)).Single(),
                    };
                }
            }

            IDocumentQuery<Document> documentQuery = _mongoDataStore.CreateDocumentQuery<Document>(
                sqlQuerySpec,
                feedOptions);

            using (documentQuery)
            {
                Debug.Assert(documentQuery != null, $"The {nameof(documentQuery)} should not be null.");

                FeedResponse<Document> fetchedResults = await documentQuery.ExecuteNextAsync<Document>(cancellationToken);

                MongoResourceWrapper[] wrappers = fetchedResults
                    .Select(r => r.GetPropertyValue<MongoResourceWrapper>(SearchValueConstants.RootAliasName)).ToArray();

                string continuationTokenId = null;

                // TODO: Eventually, we will need to take a snapshot of the search and manage the continuation
                // tokens ourselves since there might be multiple continuation token involved depending on
                // the search.
                if (!string.IsNullOrEmpty(fetchedResults.ResponseContinuation))
                {
                    continuationTokenId = await _continuationTokenCache.SaveContinuationTokenAsync(fetchedResults.ResponseContinuation, cancellationToken);
                }

                return new SearchResult(wrappers, continuationTokenId);
            }
        }
    }
}
