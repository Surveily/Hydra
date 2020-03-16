// <copyright file="CopyCommand.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Hydra.Core;
using Hydra.Core.Sharding;
using Hydra.Tools.Extensions;
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
                foreach (var sourceItem in response.Results.Where(x => string.IsNullOrWhiteSpace(Options.Object) || x.Name.EqualsCi(Options.Object)))
                {
                    _logger.LogInformation($"Processing {sourceItem.GetType().Name} '{sourceItem.Name}'.");

                    var processed = 0L;
                    var query = new TableQuery<DynamicTableEntity>();
                    var entities = await sourceItem.ExecuteQuerySegmentedAsync(query, null);

                    do
                    {
                        ApplyOverrides(entities);

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

        private void ApplyOverrides(TableQuerySegment<DynamicTableEntity> entities)
        {
            for (var i = 0; i < Options.OverrideField.Count(); i++)
            {
                var field = Options.OverrideField.ElementAt(i);
                var value = Options.OverrideValue.ElementAt(i);

                foreach (var entity in entities)
                {
                    if (field.EqualsCi(nameof(DynamicTableEntity.RowKey)))
                    {
                        entity.RowKey = value;
                    }
                    else if (field.EqualsCi(nameof(DynamicTableEntity.PartitionKey)))
                    {
                        entity.PartitionKey = value;
                    }
                    else
                    {
                        switch (entity.Properties[field].PropertyType)
                        {
                            case EdmType.Guid:
                                entity.Properties[field].GuidValue = Guid.Parse(value);
                                break;
                            case EdmType.Int32:
                                entity.Properties[field].Int32Value = int.Parse(value);
                                break;
                            case EdmType.Int64:
                                entity.Properties[field].Int64Value = long.Parse(value);
                                break;
                            case EdmType.Double:
                                entity.Properties[field].DoubleValue = double.Parse(value);
                                break;
                            case EdmType.Boolean:
                                entity.Properties[field].BooleanValue = bool.Parse(value);
                                break;
                            case EdmType.Binary:
                                entity.Properties[field].BinaryValue = Encoding.Default.GetBytes(value);
                                break;
                            case EdmType.DateTime:
                                entity.Properties[field].DateTimeOffsetValue = DateTimeOffset.Parse(value);
                                break;
                            default:
                                entity.Properties[field].StringValue = value;
                                break;
                        }
                    }
                }
            }
        }

        private async Task Copy(CloudQueueClient sourceClient, IHydra target, CancellationToken token)
        {
            var response = await sourceClient.ListQueuesSegmentedAsync(null);

            do
            {
                foreach (var sourceItem in response.Results.Where(x => string.IsNullOrWhiteSpace(Options.Object) || x.Name.EqualsCi(Options.Object)))
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
                foreach (var sourceItem in response.Results.Where(x => string.IsNullOrWhiteSpace(Options.Object) || x.Name.EqualsCi(Options.Object)).Where(x => !x.Name.StartsWith("azure-")))
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