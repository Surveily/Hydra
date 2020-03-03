// <copyright file="IStream.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hydra.Events
{
    public interface IStream
    {
        string Container { get; set; }

        Task WriteEventAsync(string shardingKey, string streamId, string eventData, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken));

        Task<string[]> ReadEventsAsync(string shardingKey, string streamId, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken));
    }
}