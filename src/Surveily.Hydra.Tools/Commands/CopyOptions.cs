// <copyright file="CopyOptions.cs" company="Surveily sp. z o.o.">
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
using Polly.Retry;

namespace Hydra.Tools.Commands
{
    [Verb("cp", HelpText = "Copies data between Azure Storage Accounts. WARNING! If you specify multiple sources or targets, you will use Hydra Jump Sharding. Very useful for re-sharding data.")]
    internal class CopyOptions : IOptions
    {
        [Option('s', "source", Required = true, HelpText = "Accounts to read from.")]
        public IEnumerable<string> Source { get; set; }

        [Option('t', "target", Required = true, HelpText = "Accounts to write to.")]
        public IEnumerable<string> Target { get; set; }

        [Option('o', "object", Required = false, HelpText = "Scope the task to single Storage object by name (eg. Table name).")]
        public string Object { get; set; }

        [Option('f', "override-fields", Required = false, HelpText = "Select which fields to override.")]
        public IEnumerable<string> OverrideField { get; set; }

        [Option('v', "override-values", Required = false, HelpText = "Set value for the overriden fields.")]
        public IEnumerable<string> OverrideValue { get; set; }

        [Option("strategy", Required = false, HelpText = "Only for internal use.")]
        public string Strategy { get; set; }

        public (List<CommandAccount> Sources, List<CommandAccount> Targets) GetAccounts()
        {
            var sourceAccounts = Source.Select(x =>
            {
                var account = CloudStorageAccount.Parse(x);

                return new CommandAccount()
                {
                    Connection = x,
                    Account = account,
                    BlobClient = account.CreateCloudBlobClient(),
                    TableClient = account.CreateCloudTableClient(),
                    QueueClient = account.CreateCloudQueueClient()
                };
            }).ToList();

            var targetAccounts = Target.Select(x =>
            {
                var account = CloudStorageAccount.Parse(x);

                return new CommandAccount()
                {
                    Connection = x,
                    Account = account,
                    BlobClient = account.CreateCloudBlobClient(),
                    TableClient = account.CreateCloudTableClient(),
                    QueueClient = account.CreateCloudQueueClient()
                };
            }).ToList();

            return (Sources: sourceAccounts, Targets: targetAccounts);
        }
    }
}