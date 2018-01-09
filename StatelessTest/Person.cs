using System;
using System.Collections.Generic;

namespace StatelessTest
{
    /// <summary>
    /// Defines the <see cref="Person" />
    /// </summary>
    [Serializable]
    public class Person
    {
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the PreferredTemperature
        /// </summary>
        public double PreferredTemperature { get; set; }

        /// <summary>
        /// Gets or sets the Devices
        /// </summary>
        public List<Device> Devices { get; set; }
    }
}
