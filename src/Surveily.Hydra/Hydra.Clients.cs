// <copyright file="Hydra.Clients.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Hydra.Core
{
    public partial class Hydra : IHydra
    {
        private readonly StorageFactory<CloudTableClient> _tableClients = new StorageFactory<CloudTableClient>(x => x.CreateCloudTableClient());

        private readonly StorageFactory<CloudBlobClient> _blobClients = new StorageFactory<CloudBlobClient>(x => x.CreateCloudBlobClient());

        private readonly StorageFactory<CloudQueueClient> _queueClients = new StorageFactory<CloudQueueClient>(x => x.CreateCloudQueueClient());

        public CloudTableClient CreateTableClient(string shardingKey)
        {
            return _tableClients.Create(PickShard(shardingKey));
        }

        public CloudBlobClient CreateBlobClient(string shardingKey)
        {
            return _blobClients.Create(PickShard(shardingKey));
        }

        public CloudQueueClient CreateQueueClient(string shardingKey)
        {
            return _queueClients.Create(PickShard(shardingKey));
        }
    }
}
