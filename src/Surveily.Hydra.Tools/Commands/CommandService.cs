// <copyright file="CommandService.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hydra.Tools.Commands
{
    internal class CommandService : BackgroundService
    {
        private readonly string[] _args;
        private readonly ILogger _logger;
        private readonly IEnumerable<ICommand> _commands;
        private readonly TaskCompletionSource<int> _result;

        public CommandService(string[] args, IEnumerable<ICommand> commands, TaskCompletionSource<int> result, ILoggerFactory loggerFactory)
        {
            _args = args;
            _result = result;
            _commands = commands;

            _logger = loggerFactory.CreateLogger<CommandService>();
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                var parser = Parser.CreateNew();

                foreach (var command in _commands)
                {
                    parser.AddCommand(command);
                }

                await parser.ParseAsync(_args, RunCommand, HandleError, token);
            }
            catch (Exception ex)
            {
                _result.TrySetException(ex);
            }
        }

        private async Task RunCommand(ICommand command, CancellationToken token)
        {
            using (StopwatchLog.LogTime(ts => _logger.LogInformation($"Finished execution in {ts}.")))
            {
                await command.RunAsync(token);
            }

            _result.TrySetResult(0);
        }

        private void HandleError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                _logger.LogError($"{error}");

                if (error.StopsProcessing)
                {
                    _result.TrySetException(new Exception($"{error}"));
                }
            }
        }
    }
}