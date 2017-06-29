using System;
using System.Collections.Generic;

namespace StatelessTest
{
    /// <summary>
    /// Stores devices such as phones, tablets, computers that can be used to determine presence.
    /// </summary>
    /// <param>The friendly name of the device
    ///     <name>Name</name>
    /// </param>
    /// <param>Contains notes / a description of the device
    ///     <name>Notes</name>
    /// </param>
    /// <param>The hardware addresses of the device, Eg Ethernet and Bluetooth
    ///     <name>Address</name>
    /// </param>
    /// that contains all the
    /// data for updating>/param>
    [Serializable]
    public class Device
    {
        public string Name { get; set; }
        public string Notes { get; set; }
        public List<string> Address { get; set; }
    }
}
