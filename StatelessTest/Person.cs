using System;
using System.Collections.Generic;

namespace StatelessTest
{
    [Serializable]
    public class Person
    {
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the preferred temperature. (Example usage scenario)
        /// </summary>
        /// <value>
        /// The persons preferred temperature.
        /// </value>
        public double PreferredTemperature { get; set; }
        public List<Device> Devices { get; set; }
    }
}
