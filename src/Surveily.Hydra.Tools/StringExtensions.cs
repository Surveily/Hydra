// <copyright file="StringExtensions.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;

namespace Hydra.Tools
{
    internal static class StringExtensions
    {
        internal static bool EqualsCi(this string a, string b)
        {
            return string.Compare(a, b, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}