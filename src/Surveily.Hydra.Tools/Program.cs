// <copyright file="Program.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

namespace Surveily.Hydra.Tools
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
