// <copyright file="ICommand.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

namespace Hydra.Tools
{
    internal interface ICommand
    {
        Type OptionsType { get; }

        Task RunAsync();
    }

    internal interface ICommand<T> : ICommand
        where T : IOptions
    {
        T Options { get; set; }
    }

    internal interface IOptions
    {
    }
}