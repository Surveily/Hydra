// <copyright file="UnitBase.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using Hydra.Core.Sharding;
using Microsoft.WindowsAzure.Storage;
using Moq;

namespace Hydra.Tests.Unit
{
    public abstract class UnitBase
    {
        static UnitBase()
        {
            Subject = CreateHydra();
        }

        public static Core.IHydra Subject { get; private set; }

        private static Core.IHydra CreateHydra()
        {
            var sharding = new Mock<ISharding>();
            sharding.Setup(x => x.GetShard(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(0);

            return Core.Hydra.Create(sharding.Object, new[] { CloudStorageAccount.DevelopmentStorageAccount });
        }
    }
}