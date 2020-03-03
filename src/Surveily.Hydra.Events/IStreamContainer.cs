// <copyright file="IStreamContainer.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Hydra.Events
{
    public interface IStreamContainer
    {
        Task<CloudAppendBlob> GetBlobReference(string shardingKey, string containerName, string streamId, CancellationToken token, StreamOptions streamOptions);
    }
}