// <copyright file="CopyCommand.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Hydra.Core;
using Hydra.Core.Sharding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using MoreLinq;
using Polly;
using Polly.Retry;

namespace Hydra.Tools.Commands
{
    internal class CopyCommand : ICommand<CopyOptions>
    {
        private readonly ILogger _logger;
        private readonly RetryPolicy _policyCreate;
        private readonly RetryPolicy _policyUpload;
        private readonly RetryPolicy _policyDownload;

        public CopyCommand(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CopyCommand>();

            _policyCreate = Policy.Handle<StorageException>(x => x.RequestInformation.HttpStatusCode == 409)
                                  .WaitAndRetryForeverAsync(x => TimeSpan.FromSeconds(3));

            _policyUpload = Policy.Handle<StorageException>(x => x.RequestInformation.ErrorCode.EqualsCi("InvalidRange")
                                                              || x.RequestInformation.HttpStatusCode == 416)
                                  .WaitAndRetryAsync(9, x => TimeSpan.FromSeconds(3), (x, t) => _logger.LogInformation($"Retrying Upload. Reason: {x.Message}."));

            _policyDownload = Policy.Handle<StorageException>(x => x.RequestInformation.ErrorCode.EqualsCi("InvalidRange")
                                                                || x.RequestInformation.HttpStatusCode == 416)
                                  .WaitAndRetryAsync(9, x => TimeSpan.FromSeconds(3), (x, t) => _logger.LogInformation($"Retrying Download. Reason: {x.Message}."));
        }

        public CopyOptions Options { get; set; }

        public Type OptionsType => typeof(CopyOptions);

        public async Task RunAsync(CancellationToken token)
        {
            var accounts = Options.GetAccounts();
            var target = Hydra.Core.Hydra.Create(new JumpSharding(), accounts.Targets.Select(x => x.Account));

            foreach (var account in accounts.Sources)
            {
                if (!token.IsCancellationRequested)
                {
                    _logger.LogInformation($"Copying tables from account: {account}");

                    await Copy(account.TableClient, target, token);

                    _logger.LogInformation($"Copying queues from account: {account}");

                    await Copy(account.QueueClient, target, token);

                    _logger.LogInformation($"Copying blob containers from account: {account}");

                    await Copy(account.BlobClient, target, token);
                }
            }
        }

        private async Task Copy(CloudTableClient source, IHydra target, CancellationToken token)
        {
            var response = await source.ListTablesSegmentedAsync(null);

            do
            {
                foreach (var sourceItem in response.Results)
                {
                    _logger.LogInformation($"Processing {sourceItem.GetType().Name} '{sourceItem.Name}'.");

                    var processed = 0L;
                    var query = new TableQuery<DynamicTableEntity>();
                    var entities = await sourceItem.ExecuteQuerySegmentedAsync(query, null);

                    do
                    {
                        foreach (var group in entities.GroupBy(x => x.PartitionKey))
                        {
                            var targetClient = target.CreateTableClient(group.Key);
                            var targetItem = targetClient.GetTableReference(sourceItem.Name);
                            await _policyCreate.ExecuteAsync(async () => await targetItem.CreateIfNotExistsAsync());

                            foreach (var batch in group.Batch(100))
                            {
                                var operation = new TableBatchOperation();

                                batch.ForEach(x => operation.Insert(x));

                                await targetItem.ExecuteBatchAsync(operation);
                            }
                        }

                        processed += entities.LongCount();

                        _logger.LogInformation($"Processed {sourceItem.GetType().Name} '{sourceItem.Name}' {processed} entities.");

                        entities = await sourceItem.ExecuteQuerySegmentedAsync(query, entities.ContinuationToken);
                    }
                    while (entities.ContinuationToken != null && !token.IsCancellationRequested);
                }

                response = await source.ListTablesSegmentedAsync(response.ContinuationToken);
            }
            while (response.ContinuationToken != null && !token.IsCancellationRequested);
        }

        private async Task Copy(CloudQueueClient sourceClient, IHydra target, CancellationToken token)
        {
            var response = await sourceClient.ListQueuesSegmentedAsync(null);

            do
            {
                foreach (var sourceItem in response.Results)
                {
                    _logger.LogInformation($"Processing {sourceItem.GetType().Name} '{sourceItem.Name}'.");

                    var targetClient = target.CreateQueueClient(sourceItem.Name);
                    var targetItem = targetClient.GetQueueReference(sourceItem.Name);
                    await _policyCreate.ExecuteAsync(async () => await targetItem.CreateIfNotExistsAsync());
                }

                response = await sourceClient.ListQueuesSegmentedAsync(response.ContinuationToken);
            }
            while (response.ContinuationToken != null && !token.IsCancellationRequested);
        }

        private async Task Copy(CloudBlobClient sourceClient, IHydra target, CancellationToken token)
        {
            var response = await sourceClient.ListContainersSegmentedAsync(null);

            do
            {
                foreach (var sourceItem in response.Results.Where(x => !x.Name.StartsWith("azure-")))
                {
                    _logger.LogInformation($"Processing {sourceItem.GetType().Name} '{sourceItem.Name}'.");

                    var sourceResponse = await sourceItem.ListBlobsSegmentedAsync(null, true, BlobListingDetails.All, null, null, null, null);

                    do
                    {
                        foreach (var sourceEntity in sourceResponse.Results.Cast<CloudBlockBlob>())
                        {
                            _logger.LogInformation($"Processing {sourceEntity.GetType().Name} '{sourceEntity.Name}'.");

                            var targetClient = target.CreateBlobClient(sourceEntity.Name.Split('/')[0]);
                            var targetItem = targetClient.GetContainerReference(sourceItem.Name);
                            await _policyCreate.ExecuteAsync(async () => await targetItem.CreateIfNotExistsAsync());

                            var targetEntity = targetItem.GetBlockBlobReference(sourceEntity.Name);

                            if (sourceEntity.Properties.Length > 0)
                            {
                                var tempFile = Path.GetTempFileName();

                                await _policyDownload.ExecuteAsync(async () =>
                                {
                                    if (File.Exists(tempFile))
                                    {
                                        File.Delete(tempFile);
                                    }

                                    await sourceEntity.DownloadToFileAsync(tempFile, FileMode.CreateNew, null, null, null, new CommandProgress(_logger, sourceEntity.Properties.Length, $"Download: {sourceEntity.Name}."), token);
                                });

                                await _policyUpload.ExecuteAsync(async () =>
                                {
                                    await targetEntity.UploadFromFileAsync(tempFile, null, null, null, new CommandProgress(_logger, sourceEntity.Properties.Length, $"Upload: {targetEntity.Name}."), token);
                                });

                                File.Delete(tempFile);
                            }
                            else
                            {
                                await _policyUpload.ExecuteAsync(async () =>
                                {
                                    await targetEntity.UploadTextAsync(string.Empty);
                                });
                            }

                            await _policyUpload.ExecuteAsync(async () =>
                            {
                                targetEntity.Properties.ContentType = sourceEntity.Properties.ContentType;

                                await targetEntity.SetPropertiesAsync();
                            });
                        }

                        sourceResponse = await sourceItem.ListBlobsSegmentedAsync(null, true, BlobListingDetails.All, null, sourceResponse.ContinuationToken, null, null);
                    }
                    while (sourceResponse.ContinuationToken != null && !token.IsCancellationRequested);
                }

                response = await sourceClient.ListContainersSegmentedAsync(response.ContinuationToken);
            }
            while (response.ContinuationToken != null && !token.IsCancellationRequested);
        }
    }
}