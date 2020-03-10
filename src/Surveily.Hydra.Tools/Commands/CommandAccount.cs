// <copyright file="CommandAccount.cs" company="Surveily sp. z o.o.">
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
    internal class CommandAccount
    {
        public string Connection { get; set; }

        public CloudStorageAccount Account { get; set; }

        public CloudBlobClient BlobClient { get; set; }

        public CloudTableClient TableClient { get; set; }

        public CloudQueueClient QueueClient { get; set; }

        public override string ToString()
        {
            return Account.BlobEndpoint.Authority;
        }
    }
}