// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Health.Fhir.Core.Exceptions;

namespace Microsoft.Health.Fhir.MongoDb.Features.Storage
{
    /// <summary>
    /// Wrapper on <see cref="IDocumentQuery"/> to provide common error status code to exceptions handling.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    public class MongoDocumentQuery<T> : IDocumentQuery<T>
    {
        private readonly IMongoQueryContext _queryContext;
        private IDocumentQuery<T> _documentQuery;
        private readonly IMongoDocumentQueryLogger _logger;

        private string _continuationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDocumentQuery{T}"/> class.
        /// </summary>
        /// <param name="queryContext">The query context.</param>
        /// <param name="documentQuery">The document query to execute.</param>
        /// <param name="logger">The logger.</param>
        public MongoDocumentQuery(
            IMongoQueryContext queryContext,
            IDocumentQuery<T> documentQuery,
            IMongoDocumentQueryLogger logger)
        {
            EnsureArg.IsNotNull(queryContext, nameof(queryContext));
            EnsureArg.IsNotNull(documentQuery, nameof(documentQuery));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _queryContext = queryContext;
            _documentQuery = documentQuery;
            _logger = logger;

            _continuationToken = _queryContext.FeedOptions?.RequestContinuation;
        }

        /// <summary>
        /// Gets a value indicating whether there are more results.
        /// </summary>
        public bool HasMoreResults => _documentQuery.HasMoreResults;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _documentQuery?.Dispose();
                _documentQuery = null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async Task<FeedResponse<TResult>> ExecuteNextAsync<TResult>(CancellationToken token = default(CancellationToken))
        {
            Guid queryId = Guid.NewGuid();

            _logger.LogQueryExecution(
                queryId,
                _queryContext.SqlQuerySpec,
                _continuationToken,
                _queryContext.FeedOptions?.MaxItemCount);

            try
            {
                FeedResponse<TResult> response = await _documentQuery.ExecuteNextAsync<TResult>(token);

                _continuationToken = response.ResponseContinuation;

                _logger.LogQueryExecutionResult(
                    queryId,
                    response.ActivityId,
                    response.RequestCharge,
                    response.ResponseContinuation,
                    response.ETag,
                    response.Count);

                return response;
            }
            catch (DocumentClientException ex)
            {
                _logger.LogQueryExecutionResult(
                    queryId,
                    ex.ActivityId,
                    ex.RequestCharge,
                    null,
                    null,
                    0,
                    ex);

                if (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    throw new ServiceUnavailableException();
                }
                else if (ex.StatusCode == (HttpStatusCode)429)
                {
                    throw new RequestRateExceededException(ex.RetryAfter);
                }

                throw;
            }
        }

        /// <inheritdoc />
        public Task<FeedResponse<dynamic>> ExecuteNextAsync(CancellationToken token = default(CancellationToken))
        {
            return ExecuteNextAsync<dynamic>(token);
        }
    }
}
