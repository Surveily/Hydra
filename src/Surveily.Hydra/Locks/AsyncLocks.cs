// <copyright file="AsyncLocks.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;

namespace Hydra.Core.Locks
{
    public class AsyncLocks<T> : IAsyncLocks<T>
    {
        private Dictionary<string, Dictionary<T, SemaphoreSlim>> _locks = new Dictionary<string, Dictionary<T, SemaphoreSlim>>();

        private object _lock = new object();

        public SemaphoreSlim Get(string collection, T key)
        {
            if (_locks.ContainsKey(collection) && _locks[collection].ContainsKey(key))
            {
                return _locks[collection][key];
            }

            lock (_lock)
            {
                if (!_locks.ContainsKey(collection))
                {
                    _locks[collection] = new Dictionary<T, SemaphoreSlim>();
                }

                if (!_locks[collection].ContainsKey(key))
                {
                    _locks[collection][key] = new SemaphoreSlim(1);
                }

                return _locks[collection][key];
            }
        }
    }
}
