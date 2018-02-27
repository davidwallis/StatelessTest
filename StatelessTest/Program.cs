using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace StatelessTest
{
    internal class Program
    {
        private static void Main()
        {
            //var home = new Location(null) { Name = "Home" };
            //var garden = new Location(home) { Name = "Garden" };

            //// ReSharper disable UnusedVariable
            //var rearGarden = new Location(garden) { Name = "Rear Garden" };

            //var frontGarden = new Location(garden) { Name = "Front Garden" };
            //var sideGarden = new Location(garden) { Name = "Side Garden" };

            //var house = new Location(home) { Name = "House" };
            //var upstairs = new Location(house) { Name = "Upstairs" };
            //var downstairs = new Location(house) { Name = "Downstairs" };

            //var frontBedroom = new Location(upstairs) { Name = "Front Bedroom" };
            //var backBedroom = new Location(upstairs) { Name = "Back Bedroom" };
            //var office = new Location(upstairs) { Name = "Office" };
            //var bathroom = new Location(upstairs) { Name = "Bathroom" };
            //var upstairsHall = new Location(upstairs) { Name = "Upstairs Hall" };

            //var kitchen = new Location(downstairs) { Name = "kitchen" };
            //var downstairsHall = new Location(downstairs) { Name = "DownstairsHall" };
            //var frontRoom = new Location(downstairs) { Name = "Front Room" };
            // ReSharper restore UnusedVariable



            var locations = new List<Location>();
            locations.Add(new Location(null) { Name = "Home" });


            locations.Add(new Location(locations.Find(x => x.Name == "Home")) { Name = "House" });
            locations.Add(new Location(locations.Find(x => x.Name == "Home")) { Name = "Garden" });

            locations.Add(new Location(locations.Find(x => x.Name == "Garden")) { Name = "Rear Garden" });
            locations.Add(new Location(locations.Find(x => x.Name == "Garden")) { Name = "Front Garden" });
            locations.Add(new Location(locations.Find(x => x.Name == "Garden")) { Name = "Side Garden" });

            locations.Add(new Location(locations.Find(x => x.Name == "Home")) { Name = "House" });
            locations.Add(new Location(locations.Find(x => x.Name == "House")) { Name = "Upstairs" });
            locations.Add(new Location(locations.Find(x => x.Name == "House")) { Name = "Downstairs" });

            locations.Add(new Location(locations.Find(x => x.Name == "Upstairs")) { Name = "Front Bedroom" });
            locations.Add(new Location(locations.Find(x => x.Name == "Upstairs")) { Name = "Back Bedroom" });
            locations.Add(new Location(locations.Find(x => x.Name == "Upstairs")) { Name = "Office" });
            locations.Add(new Location(locations.Find(x => x.Name == "Upstairs")) { Name = "Bathroom" });
            locations.Add(new Location(locations.Find(x => x.Name == "Upstairs")) { Name = "Upstairs Hall" });

            locations.Add(new Location(locations.Find(x => x.Name == "Downstairs")) { Name = "Kitchen" });
            locations.Add(new Location(locations.Find(x => x.Name == "Downstairs")) { Name = "Downstairs Hall" });
            locations.Add(new Location(locations.Find(x => x.Name == "Downstairs")) { Name = "Front Room" });

            // store locations
            BinarySerialize(locations);
            locations = null;


            // https://social.msdn.microsoft.com/Forums/en-US/c7b7dc5c-b780-49b9-95c9-b637f46c4d68/datacontractserializer-deserialize-a-class-with-a-listt?forum=csharplanguage
            // https://www.bytefish.de/blog/enums_json_net/
            // https://stackoverflow.com/questions/353558/create-pointer-to-parent-object-in-deserialization-process-in-c-sharp
            Console.WriteLine("Loading....");

            var loc = BinaryDeserialise();



            // GenerateSomeEvents(backBedroom, kitchen);
            GenerateSomeEvents2(loc.Find(x => x.Name == "Back Bedroom"), loc.Find(x => x.Name == "Kitchen"));


            Console.ReadKey();
        }

        private static void BinarySerialize(List<Location> locations)
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

        private static List<Location> BinaryDeserialise()
        {
            List<Location> deserializedLocations = null;

            // Open the file containing the data that you want to deserialize.
            using (var fs = new FileStream("DataFile.dat", FileMode.Open))
            {
                try
                {
                    var formatter = new BinaryFormatter();

                    // Deserialize the hashtable from the file and 
                    // assign the reference to the local variable.
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

        private static Device CreateTestDevice()
        {
            var d = new Device
            {
                Name = "David's Phone",
                AddressList = new List<string> { "00:AD:32:43:34:12" },
                Notes = "Davids Samsung phone"
            };
            return d;
        }

        private static void GenerateSomeEvents(Location backBedroom, Location kitchen)
        {
            const int amountOfActivity = 1;

            backBedroom.TryUpdateState(Trigger.SensorActivity);

            kitchen.TryUpdateState(Trigger.SensorActivity);
            for (var i = 0; i < amountOfActivity; i++)
            {
                //spme random activity
                if (i == 4 || i == 20)
                    backBedroom.TryUpdateState(Trigger.SensorActivity);

                kitchen.TryUpdateState(Trigger.SensorActivity);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void GenerateSomeEvents2(Location backBedroom, Location kitchen)
        {
            const int amountOfActivity = 1;

            backBedroom.TryUpdateState(Trigger.SensorActivity);

            kitchen.TryUpdateState(Trigger.SensorActivity);
            for (var i = 0; i < amountOfActivity; i++)
            {
                //spme random activity
                if (i == 4 || i == 20)
                    backBedroom.TryUpdateState(Trigger.SensorActivity);

                kitchen.TryUpdateState(Trigger.SensorActivity);
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
