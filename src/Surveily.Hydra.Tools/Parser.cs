// <copyright file="Parser.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace Hydra.Tools
{
    internal class Parser
    {
        private Dictionary<Type, object> _commandLookup = new Dictionary<Type, object>();

        private Parser()
        {
        }

        public static Parser CreateNew()
        {
            return new Parser();
        }

        public Parser AddCommand(ICommand command)
        {
            _commandLookup[command.OptionsType] = command;

            return this;
        }

        public async Task ParseAsync(IEnumerable<string> args, Func<ICommand, CancellationToken, Task> callback, Action<IEnumerable<Error>> errorCallback, CancellationToken token)
        {
            callback.ThrowIfNull(nameof(callback));
            errorCallback.ThrowIfNull(nameof(errorCallback));

            await CommandLine.Parser.Default
                .ParseArguments(args, _commandLookup.Keys.ToArray())
                .MapResult(async option =>
                {
                    var command = MatchCommandWithOption((dynamic)option);
                    await callback.Invoke(command, token);
                },
                errors =>
                {
                    errorCallback.Invoke(errors);
                    return Task.CompletedTask;
                });
        }

        private ICommand MatchCommandWithOption<T>(T options)
            where T : IOptions
        {
            var command = (ICommand<T>)_commandLookup[options.GetType()];

            command.Options = options;

            return command;
        }
    }
}