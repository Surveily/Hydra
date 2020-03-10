// <copyright file="ClearOptions.cs" company="Surveily sp. z o.o.">
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
    [Verb("rm", HelpText = "Remove all objects from target storage accounts.")]
    internal class ClearOptions : IOptions
    {
        [Option('t', "target", Required = true, HelpText = "Accounts to write to.")]
        public IEnumerable<string> Target { get; set; }

        public List<CommandAccount> GetAccounts()
        {
            return Target.Select(x =>
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
        }
    }
}