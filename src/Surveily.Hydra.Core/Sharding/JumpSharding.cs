// <copyright file="JumpSharding.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Text;
using Murmur;

namespace Hydra.Core.Sharding
{
    /// <summary>
    /// Jump Consistent Hash implementation for shard picker
    /// </summary>
    public class JumpSharding : ISharding
    {
        /// <summary>
        /// Get computed shard index
        /// </summary>
        /// <param name="key">Sharded identifier</param>
        /// <param name="buckets">Total number of shards</param>
        /// <returns>Index of a shard that is tied to the key</returns>
        public int GetShard(string key, int buckets)
        {
            var murmur128 = MurmurHash.Create32(managed: false);

            var data = murmur128.ComputeHash(Encoding.ASCII.GetBytes(key));

            return JumpConsistentHash(BitConverter.ToUInt32(data, 0), buckets);
        }

        private static int JumpConsistentHash(ulong key, int buckets)
        {
            #pragma warning disable SA1119
            #pragma warning disable SA1407
            #pragma warning disable SA1139
            long b = 1;
            long j = 0;

            while (j < buckets)
            {
                b = j;
                key = key * 2862933555777941757 + 1;

                var x = (double)(b + 1);
                var y = (double)(((long)(1)) << 31);
                var z = (double)((key >> 33) + 1);

                j = (long)(x * (y / z));
            }

            return (int)b;
            #pragma warning restore SA1119
            #pragma warning restore SA1407
            #pragma warning restore SA1139
        }
    }
}
