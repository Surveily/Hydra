// <copyright file="ClearCommand.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Tools.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Polly;
using Polly.Retry;

namespace Hydra.Tools.Commands
{
    internal class ClearCommand : ICommand<ClearOptions>
    {
        private readonly ILogger _logger;
        private readonly RetryPolicy _policyCreate;

        public ClearCommand(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ClearCommand>();

            _policyCreate = Policy.Handle<StorageException>()
                                  .WaitAndRetryForeverAsync(x => TimeSpan.FromSeconds(3));
        }

        public ClearOptions Options { get; set; }

        public Type OptionsType => typeof(ClearOptions);

        public async Task RunAsync(CancellationToken token)
        {
            var accounts = Options.GetAccounts();

            foreach (var account in accounts)
            {
                if (!token.IsCancellationRequested)
                {
                    _logger.LogInformation($"Clearing tables from account: {account}");

                    await Clear(account.TableClient, token);

                    _logger.LogInformation($"Clearing queues from account: {account}");

                    await Clear(account.QueueClient, token);

                    _logger.LogInformation($"Clearing blob containers from account: {account}");

                    await Clear(account.BlobClient, token);
                }
            }
        }

        private async Task Clear(CloudTableClient target, CancellationToken token)
        {
            var response = await target.ListTablesSegmentedAsync(null);

            do
            {
                foreach (var item in response.Results.Where(x => string.IsNullOrWhiteSpace(Options.Object) || x.Name.EqualsCi(Options.Object)))
                {
                    var targetItem = target.GetTableReference(item.Name);

                    _logger.LogInformation($"Deleting {item.GetType().Name} '{item.Name}'...");

                    await targetItem.DeleteAsync();
                }

                response = await target.ListTablesSegmentedAsync(response.ContinuationToken);
            }
            while (response.ContinuationToken != null && !token.IsCancellationRequested);
        }

        private async Task Clear(CloudQueueClient target, CancellationToken token)
        {
            var response = await target.ListQueuesSegmentedAsync(null);

            do
            {
                foreach (var sourceItem in response.Results.Where(x => string.IsNullOrWhiteSpace(Options.Object) || x.Name.EqualsCi(Options.Object)))
                {
                    var targetItem = target.GetQueueReference(sourceItem.Name);

                    _logger.LogInformation($"Deleting {sourceItem.GetType().Name} '{sourceItem.Name}'...");

                    await targetItem.DeleteAsync();
                }

                response = await target.ListQueuesSegmentedAsync(response.ContinuationToken);
            }
            while (response.ContinuationToken != null && !token.IsCancellationRequested);
        }

        private async Task Clear(CloudBlobClient target, CancellationToken token)
        {
            var response = await target.ListContainersSegmentedAsync(null);

            do
            {
                foreach (var sourceItem in response.Results.Where(x => string.IsNullOrWhiteSpace(Options.Object) || x.Name.EqualsCi(Options.Object)).Where(x => !x.Name.StartsWith("azure-")))
                {
                    var targetItem = target.GetContainerReference(sourceItem.Name);

                    _logger.LogInformation($"Deleting {sourceItem.GetType().Name} '{sourceItem.Name}'...");

                    await targetItem.DeleteAsync();
                }

                response = await target.ListContainersSegmentedAsync(response.ContinuationToken);
            }
            while (response.ContinuationToken != null && !token.IsCancellationRequested);
        }
    }
}