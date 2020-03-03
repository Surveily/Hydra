// <copyright file="StreamContainer.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Core;
using Hydra.Core.Locks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Hydra.Events
{
    public class StreamContainer : IStreamContainer
    {
        private static readonly IDictionary<string, object> ExistingContainers = new ConcurrentDictionary<string, object>();

        private static readonly IDictionary<string, object> ExistingStreams = new ConcurrentDictionary<string, object>();

        private static readonly IAsyncLocks<string> Locks = new AsyncLocks<string>();

        private readonly IHydra _hydra;

        public StreamContainer(IHydra hydra)
        {
            _hydra = hydra;
        }

        public async Task<CloudAppendBlob> GetBlobReference(string shardingKey, string containerName, string streamId, CancellationToken token, StreamOptions streamOptions)
        {
            var client = _hydra.CreateBlobClient(shardingKey);
            var account = client.Credentials.AccountName;
            var container = client.GetContainerReference(containerName);
            var semaphore = GetSemaphore(account, containerName, streamId);

            if (await semaphore.WaitAsync(TimeSpan.FromSeconds(5), token))
            {
                try
                {
                    if (streamOptions.CreateContainer && !GetContainerExists(account, containerName))
                    {
                        await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null, token);
                        SetContainerExists(account, containerName);
                    }

                    var blob = container.GetAppendBlobReference(streamId);

                    if (streamOptions.CreateBlob && !GetStreamExists(account, containerName, streamId) && !await blob.ExistsAsync(null, null, token))
                    {
                        await blob.CreateOrReplaceAsync(null, null, null, token);
                        SetStreamExists(account, containerName, streamId);
                    }

                    return blob;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            throw new TimeoutException("Unable to get blob reference");
        }

        private static SemaphoreSlim GetSemaphore(string account, string container, string blob)
        {
            return Locks.Get($"{account}-{container}", blob);
        }

        private static bool GetContainerExists(string account, string container)
        {
            return ExistingContainers.ContainsKey($"{account}-{container}");
        }

        private static void SetContainerExists(string account, string container)
        {
            ExistingContainers[$"{account}-{container}"] = true;
        }

        private static bool GetStreamExists(string account, string container, string blob)
        {
            return ExistingStreams.ContainsKey($"{account}-{container}-{blob}");
        }

        private static void SetStreamExists(string account, string container, string blob)
        {
            ExistingStreams[$"{account}-{container}-{blob}"] = true;
        }
    }
}
