// <copyright file="StreamOptions.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;

namespace Hydra.Events
{
    public class StreamOptions
    {
        public StreamOptions()
        {
            CreateContainer = true;
            CreateBlob = true;
            AppendDelimeter = true;
        }

        public bool CreateContainer { get; set; }

        public bool CreateBlob { get; set; }

        public bool AppendDelimeter { get; set; }
    }
}
