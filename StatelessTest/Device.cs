using System;
using System.Collections.Generic;

namespace StatelessTest
{
    /// <summary>
    /// Defines the <see cref="Device" />
    /// </summary>
    [Serializable]
    public class Device
    {
        /// <summary>
        /// Gets or sets the name of the device
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the List of Addresses
        /// </summary>
        public List<string> AddressList { get; set; }
    }
}
