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

namespace Hydra.Tools.Commands
{
    internal class CommandService : BackgroundService
    {
        private readonly string[] _args;
        private readonly IEnumerable<ICommand> _commands;
        private readonly TaskCompletionSource<int> _result;

        public CommandService(string[] args, IEnumerable<ICommand> commands, TaskCompletionSource<int> result)
        {
            _args = args;
            _result = result;
            _commands = commands;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var parser = Parser.CreateNew();

            foreach (var command in _commands)
            {
                parser.AddCommand(command);
            }

            await parser.ParseAsync(_args, RunCommand, HandleError);
        }

        private async Task RunCommand(ICommand command)
        {
            using (StopwatchLog.LogTime(ts => Console.WriteLine($"Finished execution in {ts}.")))
            {
                await command.RunAsync();
            }

            _result.TrySetResult(0);
        }

        private void HandleError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error);

                if (error.StopsProcessing)
                {
                    _result.TrySetException(new Exception($"{error}"));
                }
            }
        }
    }
}