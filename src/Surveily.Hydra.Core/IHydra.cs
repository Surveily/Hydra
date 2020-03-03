// <copyright file="IHydra.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Hydra.Core.Sharding;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Hydra.Core
{
    /// <summary>
    /// Hydra Storage Engine
    /// </summary>
    public interface IHydra
    {
        /// <summary>
        /// Selected sharding algorithm
        /// </summary>
        ISharding Sharding { get; }

        /// <summary>
        /// Storage Accounts Hash Space
        /// </summary>
        IEnumerable<CloudStorageAccount> Accounts { get; }

        /// <summary>
        /// Add a Storage Account to the Hash Space
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        IHydra AddAccount(CloudStorageAccount account);

        /// <summary>
        /// Set sharding algorithm
        /// </summary>
        /// <param name="sharding"></param>
        /// <returns></returns>
        IHydra SetSharding(ISharding sharding);

        /// <summary>
        /// Create a TableClient for a specific shard
        /// </summary>
        /// <param name="shardingKey">Key which determines which shard to use</param>
        CloudTableClient CreateTableClient(string shardingKey);

        /// <summary>
        /// Create a BlobClient for a specific shard
        /// </summary>
        /// <param name="shardingKey">Key which determines which shard to use</param>
        CloudBlobClient CreateBlobClient(string shardingKey);

        /// <summary>
        /// Create a QueueClient for a specific shard
        /// </summary>
        /// <param name="shardingKey">Key which determines which shard to use</param>
        CloudQueueClient CreateQueueClient(string shardingKey);
    }
}