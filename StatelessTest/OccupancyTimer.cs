// <copyright file="OccupancyTimer.cs" company="David Wallis">
// Copyright (c) David Wallis. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// https://www.github.com/davidwallis3101
// </copyright>

using System.Timers;
using StatelessTest.Interfaces;

namespace StatelessTest
{
    /// <summary>
    /// Wraps the timer and implements the <see langword="interface"/>
    /// </summary>
    public class OccupancyTimer : Timer, ITimer
    {
        // Make a timer that implements the interface and exposes the System.Timer
        // https://www.codeproject.com/Tips/1068865/Unit-Testing-Classes-That-Use-the-System-Timer
    }
}
