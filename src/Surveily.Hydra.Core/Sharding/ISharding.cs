// <copyright file="ISharding.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

namespace Hydra.Core.Sharding
{
    /// <summary>
    /// Shard picker definition
    /// </summary>
    public interface ISharding
    {
        /// <summary>
        /// Get computed shard index
        /// </summary>
        /// <param name="key">Sharded identifier</param>
        /// <param name="buckets">Total number of shards</param>
        /// <returns>Index of a shard that is tied to the key</returns>
        int GetShard(string key, int buckets);
    }
}
