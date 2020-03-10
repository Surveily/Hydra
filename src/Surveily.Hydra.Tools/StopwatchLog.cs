// <copyright file="StopwatchLog.cs" company="Surveily sp. z o.o.">
// Copyright (c) Surveily sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace Hydra.Tools
{
    internal class StopwatchLog : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly Action<TimeSpan> _endCallback;

        private StopwatchLog(Action<TimeSpan> endCallback)
        {
            _stopwatch = Stopwatch.StartNew();
            _endCallback = endCallback;
        }

        public TimeSpan ElapsedTime => _stopwatch.Elapsed;

        public static StopwatchLog LogTime(Action<TimeSpan> callback = null)
        {
            return new StopwatchLog(callback);
        }

        public void Stop()
        {
            _stopwatch.Stop();

            _endCallback?.Invoke(ElapsedTime);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}