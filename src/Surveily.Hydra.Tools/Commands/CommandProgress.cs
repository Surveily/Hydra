// <copyright file="CommandProgress.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Microsoft.WindowsAzure.Storage.Core.Util;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using MoreLinq;

namespace Hydra.Tools.Commands
{
    internal class CommandProgress : IProgress<StorageProgress>
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly string _msg;
        private readonly long _bytes;

        private decimal _previous;

        public CommandProgress(Microsoft.Extensions.Logging.ILogger logger, long bytes, string msg)
        {
            _msg = msg;
            _bytes = bytes;
            _logger = logger;
        }

        public void Report(StorageProgress value)
        {
            var current = ((decimal)value.BytesTransferred / (decimal)_bytes) * 100.0m;

            if (_previous < current - 10)
            {
                _logger.LogInformation($"{_msg} Progress {Math.Floor(current)}%");

                _previous = current;
            }
        }
    }
}