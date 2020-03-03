// <copyright file="Shard.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.WindowsAzure.Storage;

namespace Hydra.Core.Sharding
{
    public class Shard
    {
        public int Index { get; set; }

        public CloudStorageAccount Account { get; set; }
    }
}