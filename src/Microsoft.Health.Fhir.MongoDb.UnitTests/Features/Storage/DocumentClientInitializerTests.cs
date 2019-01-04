// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Health.Fhir.Core.Features.Context;
using Microsoft.Health.Fhir.MongoDb.Configs;
using Microsoft.Health.Fhir.MongoDb.Features.Storage;
using Microsoft.Health.Fhir.MongoDb.Features.Storage.Versioning;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.MongoDb.UnitTests.Features.Storage
{
    public class DocumentClientInitializerTests
    {
        private readonly DocumentClientInitializer _documentClientInitializer = new DocumentClientInitializer(Substitute.For<IDocumentClientTestProvider>(), NullLogger<DocumentClientInitializer>.Instance, Substitute.For<IUpgradeManager>(), Substitute.For<IFhirRequestContextAccessor>());
        private readonly MongoDataStoreConfiguration _mongoDataStoreConfiguration;

        public DocumentClientInitializerTests()
        {
            _mongoDataStoreConfiguration = new MongoDataStoreConfiguration
            {
                AllowDatabaseCreation = false,
                CollectionId = "testcollectionid",
                ConnectionMode = Azure.Documents.Client.ConnectionMode.Direct,
                ConnectionProtocol = Azure.Documents.Client.Protocol.Https,
                DatabaseId = "testdatabaseid",
                Host = "https://fakehost",
                Key = "ZmFrZWtleQ==",   // "fakekey"
                PreferredLocations = null,
            };

            _mongoDataStoreConfiguration.RetryOptions.MaxNumberOfRetries = 10;
            _mongoDataStoreConfiguration.RetryOptions.MaxWaitTimeInSeconds = 99;
        }

        [Fact]
        public void CreateDocumentClient_NullPreferredLocations_DoesNotSetPreferredLocations()
        {
            var documentClient = _documentClientInitializer.CreateDocumentClient(_mongoDataStoreConfiguration);

            Assert.NotNull(documentClient.ConnectionPolicy);
            Assert.Empty(documentClient.ConnectionPolicy.PreferredLocations);
        }

        [Fact]
        public void CreateDocumentClient_EmptyPreferredLocations_DoesNotSetPreferredLocations()
        {
            _mongoDataStoreConfiguration.PreferredLocations = new string[] { };
            var documentClient = _documentClientInitializer.CreateDocumentClient(_mongoDataStoreConfiguration);

            Assert.NotNull(documentClient.ConnectionPolicy);
            Assert.Empty(documentClient.ConnectionPolicy.PreferredLocations);
        }

        [Fact]
        public void CreateDocumentClient_SetsPreferredLocations()
        {
            _mongoDataStoreConfiguration.PreferredLocations = new[] { "southcentralus", "northcentralus" };
            var documentClient = _documentClientInitializer.CreateDocumentClient(_mongoDataStoreConfiguration);

            Assert.NotNull(documentClient.ConnectionPolicy);
            Assert.NotEmpty(documentClient.ConnectionPolicy.PreferredLocations);
            Assert.Equal(2, documentClient.ConnectionPolicy.PreferredLocations.Count);

            for (int i = 0; i < _mongoDataStoreConfiguration.PreferredLocations.Count; i++)
            {
                Assert.Equal(_mongoDataStoreConfiguration.PreferredLocations[i], documentClient.ConnectionPolicy.PreferredLocations[i]);
            }
        }

        [Fact]
        public void CreateDocumentClient_SetsMaxRetryAttemptsOnThrottledRequests()
        {
            var documentClient = _documentClientInitializer.CreateDocumentClient(_mongoDataStoreConfiguration);

            Assert.NotNull(documentClient.ConnectionPolicy);
            Assert.NotNull(documentClient.ConnectionPolicy.RetryOptions);
            Assert.Equal(10, documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests);
        }

        [Fact]
        public void CreateDocumentClient_SetsMaxRetryWaitTimeInSeconds()
        {
            var documentClient = _documentClientInitializer.CreateDocumentClient(_mongoDataStoreConfiguration);

            Assert.NotNull(documentClient.ConnectionPolicy);
            Assert.NotNull(documentClient.ConnectionPolicy.RetryOptions);
            Assert.Equal(99, documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds);
        }
    }
}
