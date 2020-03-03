// <copyright file="Stream.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hydra.Events
{
    public class Stream : IStream
    {
        private static readonly string DelimiterString = "\r\n";

        private readonly IStreamContainer _streamContainer;

        public Stream(IStreamContainer streamContainer, string container = "hydra")
        {
            _streamContainer = streamContainer;
            Container = container;
        }

        public string Container { get; set; }

        public async Task WriteEventAsync(string shardingKey, string streamId, string eventData, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var dataBuilder = new StringBuilder(eventData);
            var streamOptions = options ?? new StreamOptions();
            var blob = await _streamContainer.GetBlobReference(shardingKey, Container, streamId, token, streamOptions);

            if (streamOptions.AppendDelimeter)
            {
                dataBuilder.Append(DelimiterString);
            }

            using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(dataBuilder.ToString())))
            {
                await blob.AppendBlockAsync(dataStream, null, null, null, null, token);
            }
        }

        public async Task<string[]> ReadEventsAsync(string shardingKey, string streamId, StreamOptions options = default(StreamOptions), CancellationToken token = default(CancellationToken))
        {
            var streamOptions = options ?? new StreamOptions();
            var blob = await _streamContainer.GetBlobReference(shardingKey, Container, streamId, token, streamOptions);
            var content = await blob.DownloadTextAsync(Encoding.UTF8, null, null, null, token);

            return content.Split(new[] { DelimiterString }, StringSplitOptions.None);
        }
    }
}
