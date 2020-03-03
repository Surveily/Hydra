// <copyright file="StorageFactory.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using Hydra.Core.Sharding;
using Microsoft.WindowsAzure.Storage;

namespace Hydra.Core
{
    public class StorageFactory<T>
    {
        private readonly Func<CloudStorageAccount, T> _createFunc;

        private readonly ConcurrentDictionary<int, T> _items = new ConcurrentDictionary<int, T>();

        public StorageFactory(Func<CloudStorageAccount, T> createFunc)
        {
            _createFunc = createFunc;
        }

        public T Create(Shard shard)
        {
            T result;

            if (_items.TryGetValue(shard.Index, out result))
            {
                return result;
            }

            result = _createFunc(shard.Account);

            _items.AddOrUpdate(shard.Index, result, (i, arg2) => result);

            return result;
        }
    }
}