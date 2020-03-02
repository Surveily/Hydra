// <copyright file="IAsyncLocks.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System.Threading;

namespace Hydra.Core.Locks
{
    public interface IAsyncLocks<T>
    {
        SemaphoreSlim Get(string collection, T key);
    }
}