// <copyright file="NullCheckExtensions.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;

namespace Hydra.Tools
{
    public static class NullCheckExtensions
    {
        public static T ThrowIfNull<T>(this T subject, string message)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(message);
            }

            return subject;
        }
    }
}