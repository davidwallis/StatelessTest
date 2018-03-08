// <copyright file="ITimer.cs" company="David Wallis">
// Copyright (c) David Wallis. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// https://www.github.com/davidwallis3101
// </copyright>

using System.Timers;

namespace StatelessTest.Interfaces
{
    /// <summary>
    /// Interface for the <see cref="OccupancyTimer"/>/>
    /// </summary>
    public interface ITimer
    {
        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        event ElapsedEventHandler Elapsed;

        /// <summary>
        /// Gets or sets the interval at which to raise the System.Timers.Timer.Elapsed event.
        /// </summary>
        double Interval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the timer is enabled or not.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Disposes of the timer
        /// </summary>
        void Dispose();

        /// <summary>
        /// Starts raising the Elapsed event by setting Enabled to true.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops raising the Elapsed event by setting Enabled to false.
        /// </summary>
        void Stop();

}
}