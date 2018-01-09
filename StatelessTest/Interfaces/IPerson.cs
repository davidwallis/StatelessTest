using System.Collections.Generic;

namespace StatelessTest
{
    public interface IPerson
    {
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the preferred temperature. (Example usage scenario)
        /// </summary>
        /// <value>
        /// The persons preferred temperature.
        /// </value>
        double PreferredTemperature { get; set; }

        /// <summary>
        /// Gets or sets the Devices
        /// </summary>
        List<Device> Devices { get; set; }
    }
}