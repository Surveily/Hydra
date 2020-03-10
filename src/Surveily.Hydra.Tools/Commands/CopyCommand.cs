// <copyright file="CopyCommand.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
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

        public async Task RunAsync()
        {
            var accounts = Options.GetAccounts();

            await Task.Delay(1);
        }
    }
}