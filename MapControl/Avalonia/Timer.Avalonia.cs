﻿using Avalonia;
using Avalonia.Threading;
using System;

namespace MapControl
{
    internal static class Timer
    {
        public static DispatcherTimer CreateTimer(this AvaloniaObject obj, TimeSpan interval)
        {
            var timer = new DispatcherTimer
            {
                Interval = interval
            };

            return timer;
        }

        public static void Run(this DispatcherTimer timer, bool restart = false)
        {
            if (restart)
            {
                timer.Stop();
            }

            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }
    }
}