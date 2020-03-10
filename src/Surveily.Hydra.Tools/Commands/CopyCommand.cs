// <copyright file="CopyCommand.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Retry;

namespace Hydra.Tools.Commands
{
    internal class CopyCommand : ICommand<CopyOptions>
    {
        private readonly ILogger _logger;

        public CopyCommand(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CopyCommand>();
        }

        public CopyOptions Options { get; set; }

        public Type OptionsType => typeof(CopyOptions);

        public async Task RunAsync()
        {
            await Task.Delay(1000);
        }
    }

    [Verb("cp")]
    internal class CopyOptions : IOptions
    {
        [Option('s', "source", Required = true, HelpText = "Accounts to read from.")]
        public IEnumerable<string> Source { get; set; }

        [Option('t', "target", Required = true, HelpText = "Accounts to write to.")]
        public IEnumerable<string> Target { get; set; }
    }
}