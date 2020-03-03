// <copyright file="IntegrationBase.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using Hydra.Core;
using Hydra.Core.Sharding;
using Microsoft.WindowsAzure.Storage;

namespace Hydra.Tests.Integration
{
    public abstract class IntegrationBase
    {
        public const string TableName = "testtable";
        public const string ContainerName = "testcontainer";
        public const string QueueName = "testqueue";

        public static string TestKey = Guid.NewGuid().ToString();

        static IntegrationBase()
        {
            Subject = CreateHydra();
        }

        public static IHydra Subject { get; private set; }

        protected static IHydra CreateHydra()
        {
            var sharding = new JumpSharding();

            return Core.Hydra.Create(sharding, new[]
            {
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST")),
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST")),
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST")),
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST")),
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("HYDRATEST"))
            });
        }
    }
}
