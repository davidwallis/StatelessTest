using System;
using System.Collections.Generic;
using System.IO;
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


            locations.Add(new Location(locations.Find(x => x.Name == "home")) { Name = "House" });
            locations.Add(new Location(locations.Find(x => x.Name == "home")) { Name = "Garden" });

            locations.Add(new Location(locations.Find(x => x.Name == "garden")) { Name = "Rear Garden" });
            locations.Add(new Location(locations.Find(x => x.Name == "garden")) { Name = "Front Garden" });
            locations.Add(new Location(locations.Find(x => x.Name == "garden")) { Name = "Side Garden" });

            locations.Add(new Location(locations.Find(x => x.Name == "home")) { Name = "House" });
            locations.Add(new Location(locations.Find(x => x.Name == "house")) { Name = "Upstairs" });
            locations.Add(new Location(locations.Find(x => x.Name == "house")) { Name = "Downstairs" });

            locations.Add(new Location(locations.Find(x => x.Name == "upstairs")) { Name = "Front Bedroom" });
            locations.Add(new Location(locations.Find(x => x.Name == "upstairs")) { Name = "Back Bedroom" });
            locations.Add(new Location(locations.Find(x => x.Name == "upstairs")) { Name = "Office" });
            locations.Add(new Location(locations.Find(x => x.Name == "upstairs")) { Name = "Bathroom" });
            locations.Add(new Location(locations.Find(x => x.Name == "upstairs")) { Name = "Upstairs Hall" });

            locations.Add(new Location(locations.Find(x => x.Name == "downstairs")) { Name = "Kitchen" });
            locations.Add(new Location(locations.Find(x => x.Name == "downstairs")) { Name = "Downstairs Hall" });
            locations.Add(new Location(locations.Find(x => x.Name == "downstairs")) { Name = "Front Room" });

            // store locations
            SaveSettings<List<Location>>(locations, Path.Combine(Environment.CurrentDirectory, "locations.xml"));


            // https://social.msdn.microsoft.com/Forums/en-US/c7b7dc5c-b780-49b9-95c9-b637f46c4d68/datacontractserializer-deserialize-a-class-with-a-listt?forum=csharplanguage

            var locNew = LoadSettings<List<Location>>(Path.Combine(Environment.CurrentDirectory, "locations.xml"));

            //Create a device
            var device = CreateTestDevice();
            // Save device to devices.xml
            //SaveSettings<Device>(device, Path.Combine(Environment.CurrentDirectory, "devices.xml"));

            // try Loading XML

            //var dNew = LoadSettings<Device>(Path.Combine(Environment.CurrentDirectory, "devices.xml"));
            //var children = garden.AllChildren;

            //foreach (var childArea in children)
            //{
            //    Console.WriteLine(childArea.Name);
            //}


            // GenerateSomeEvents(backBedroom, kitchen);
            GenerateSomeEvents2(locations.Find(x => x.Name == "Back Bedroom"), locations.Find(x => x.Name == "Kitchen"));


            Console.ReadKey();
        }

        private static void SaveSettings<T>(object d, string filePath)
        {
            // export
            var writerSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            var serializer = new XmlSerializer(typeof(T));

            //// Create an XmlTextWriter using a FileStream.
            //Stream fs = new FileStream(filePath, FileMode.Create);
            //var writer = XmlWriter.Create(fs, writerSettings);
            //serializer.Serialize(writer, d);
            //writer.Close();

            using (StreamWriter sw = new StreamWriter(File.Open(filePath, FileMode.Create)))
            {
                var writer = XmlWriter.Create(sw, writerSettings);
                serializer.Serialize(writer, d);
            }
        }

        private static T LoadSettings<T>(string filePath)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StreamReader(filePath))
            {
                return (T)serializer.Deserialize(reader);
            }
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
