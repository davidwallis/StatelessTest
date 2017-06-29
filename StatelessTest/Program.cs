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
            var home = new Location(null) { Name = "Home" };
            var garden = new Location(home) { Name = "Garden" };
            var rearGarden = new Location(garden) { Name = "Rear Garden" };
            var frontGarden = new Location(garden) { Name = "Front Garden" };
            var sideGarden = new Location(garden) { Name = "Side Garden" };

            var house = new Location(home) { Name = "House" };
            var upstairs = new Location(house) { Name = "Upstairs" };
            var downstairs = new Location(house) { Name = "Downstairs" };

            var frontBedroom = new Location(upstairs) { Name = "Front Bedroom" };
            var backBedroom = new Location(upstairs) { Name = "Back Bedroom" };
            var office = new Location(upstairs) { Name = "Office" };
            var bathroom = new Location(upstairs) { Name = "Bathroom" };
            var upstairsHall = new Location(upstairs) { Name = "Upstairs Hall" };

            var kitchen = new Location(downstairs) { Name = "kitchen" };
            var downstairsHall = new Location(downstairs) { Name = "DownstairsHall" };
            var frontRoom = new Location(downstairs) { Name = "Front Room" };

            //Create a device
            var device = CreateTestDevice();

            // Save device to devices.xml
            SaveSettings<Device>(device, Path.Combine(Environment.CurrentDirectory, "devices.xml"));

            // try Loading XML

            var dNew = LoadSettings<Device>(Path.Combine(Environment.CurrentDirectory, "devices.xml"));

            var children = garden.AllChildren;

            foreach (var childArea in children)
            {
                Console.WriteLine(childArea.Name);
            }

            //OccupancyStateMachine occupancy = new OccupancyStateMachine();
            GenerateSomeEvents(backBedroom, kitchen);

            //Console.WriteLine("Occupancy status: {0}", occupancy.Status);
            //var result = occupancy.TryUpdateState(Trigger.SensorActivity);

            //Console.WriteLine("Attempt to trigger activity: {0}", result);
            //Console.WriteLine("Occupancy status now : {0}", occupancy.Status);

            System.Threading.Thread.Sleep(10000);

            //result = occupancy.TryUpdateState(Trigger.AlarmPartSet);
            //result = occupancy.TryUpdateState(Trigger.AlarmFullSet);

            //Console.WriteLine("Attempt to part set alarm: {0}", result);
            //Console.WriteLine("Occupancy status: {0}", occupancy.Status);

            // result = occupancy.TryUpdateState(Trigger.AlarmUnset);
            //Console.WriteLine("Attemp to unset alarm: {0}", result);
            //Console.WriteLine("Occupancy status: {0}", occupancy.Status);

            Console.WriteLine("Press any key");
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

            // Create an XmlTextWriter using a FileStream.
            Stream fs = new FileStream(filePath, FileMode.Create);
            var writer = XmlWriter.Create(fs, writerSettings);
            serializer.Serialize(writer, d);
            writer.Close();
        }

        private static T LoadSettings<T>(string filePath)
        {
            var serializer = new XmlSerializer(typeof(T));
            var reader = new StreamReader(filePath);
            return (T)serializer.Deserialize(reader);
        }

        private static Device CreateTestDevice()
        {
            var d = new Device
            {
                Name = "David's Phone",
                Address = new List<string> { "00:AD:32:43:34:12" },
                Notes = "Davids Samsung phone"
            };
            return d;
        }

        private static void GenerateSomeEvents(Location backBedroom, Location kitchen)
        {
            backBedroom.TryUpdateState(Trigger.SensorActivity);

            kitchen.TryUpdateState(Trigger.SensorActivity);
            for (int i = 0; i < 30; i++)
            {
                if (i == 4 || i == 20)
                    backBedroom.TryUpdateState(Trigger.SensorActivity);

                kitchen.TryUpdateState(Trigger.SensorActivity);
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
