// <copyright file="NullCheckExtensions.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;

namespace Hydra.Tools
{
    internal static class NullCheckExtensions
    {
        internal static T ThrowIfNull<T>(this T subject, string message)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(message);
            }

            return subject;
        }
    }
}