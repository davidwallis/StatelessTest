using System;
using System.Collections.Generic;

namespace StatelessTest
{
    internal class Program
    {
        private static void Main()
        {

            // Create some test data and serialize
            GenerateAndSerialize.GenerateAndSerializeData();

            // https://social.msdn.microsoft.com/Forums/en-US/c7b7dc5c-b780-49b9-95c9-b637f46c4d68/datacontractserializer-deserialize-a-class-with-a-listt?forum=csharplanguage
            // https://www.bytefish.de/blog/enums_json_net/
            // https://stackoverflow.com/questions/353558/create-pointer-to-parent-object-in-deserialization-process-in-c-sharp

            Console.WriteLine("Loading data....");
            var loc = SerializationHelper.BinaryDeserialise();

            Console.WriteLine("\nDisplay using: http://www.webgraphviz.com/\n");
            Console.WriteLine(loc.Find(x => x.Name == "RoundhayCrescent").StateMachineAsDotGraph + "\n");
            
            SerializationHelper.BinarySerialize(loc);
            loc.Find(x => x.Name == "Kitchen").TryUpdateState(Trigger.SensorActivity);
            //foreach (var l in loc)
            //{
            //    string parentName = string.Empty;
            //    if (l.Parent == null)
            //    {
            //        parentName = "Root";
            //    }
            //    else
            //    {
            //        parentName = l.Parent.Name;
            //    }
            //    Console.WriteLine($"Location: {l.Name} Parent: {parentName} State: {l.OccupancyState}");
            //}


            Console.WriteLine("Press any key to generate data\n");
            Console.ReadKey();
            // GenerateSomeEvents(backBedroom, kitchen);
            GenerateSomeEvents2(loc.Find(x => x.Name == "Back Bedroom"), loc.Find(x => x.Name == "Kitchen"));

            Console.WriteLine("Press any key to exit\n");
            Console.ReadKey();
        }

        //private static void BinarySerialize(List<Location> locations)
        //{
        //    using (var fs = new FileStream("DataFile.dat", FileMode.Create))
        //    {
        //        var formatter = new BinaryFormatter();
        //        try
        //        {
        //            formatter.Serialize(fs, locations);
        //        }
        //        catch (SerializationException e)
        //        {
        //            Console.WriteLine("Failed to serialize. Reason: " + e.Message);
        //            throw;
        //        }
        //        finally
        //        {
        //            fs.Close();
        //        }
        //    }
        //}

        //private static List<Location> BinaryDeserialise()
        //{
        //    List<Location> deserializedLocations = null;

        //    // Open the file containing the data that you want to deserialize.
        //    using (var fs = new FileStream("DataFile.dat", FileMode.Open))
        //    {
        //        try
        //        {
        //            var formatter = new BinaryFormatter();

        //            // Deserialize the hashtable from the file and 
        //            // assign the reference to the local variable.
        //            deserializedLocations = (List<Location>)formatter.Deserialize(fs);
        //        }
        //        catch (SerializationException e)
        //        {
        //            Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
        //            throw;
        //        }
        //        finally
        //        {
        //            fs.Close();
        //        }
        //    }


        //    return deserializedLocations;
        //}

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
