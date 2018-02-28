using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace StatelessTest
{
    class SerializationHelper
    {
        internal static void BinarySerialize(List<Location> locations)
        {
            using (var fs = new FileStream("DataFile.dat", FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, locations);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        internal static List<Location> BinaryDeserialise()
        {
            List<Location> deserializedLocations = null;

            // Open the file containing the data that you want to deserialize.
            using (var fs = new FileStream("DataFile.dat", FileMode.Open))
            {
                try
                {
                    var formatter = new BinaryFormatter();
                    deserializedLocations = (List<Location>)formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }


            return deserializedLocations;
        }
    }
}
