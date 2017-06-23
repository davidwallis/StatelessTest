using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatelessTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var home = new Area(null) { Name = "Home" };
            var garden = new Area(home) { Name = "Garden" };
            var rearGarden = new Area(garden) { Name = "Rear Garden" };
            var frontGarden = new Area(garden) { Name = "Front Garden" };
            var sideGarden = new Area(garden) { Name = "Side Garden" };

            var house = new Area(home) { Name = "House" };
            var upstairs = new Area(house) { Name = "Upstairs" };
            var downstairs = new Area(house) { Name = "Downstairs" };

            var frontBedroom = new Area(upstairs) { Name = "Front Bedroom" };
            var backBedroom = new Area(upstairs) { Name = "Back Bedroom" };
            var office = new Area(upstairs) { Name = "Office" };
            var bathroom = new Area(upstairs) { Name = "Bathroom" };
            var upstairsHall = new Area(upstairs) { Name = "Upstairs Hall" };

            var kitchen = new Area(downstairs) { Name = "kitchen" };
            var downstairsHall = new Area(downstairs) { Name = "DownstairsHall" };
            var frontRoom = new Area(downstairs) { Name = "Front Room" };

            var children = garden.AllChildren;
            foreach (var childArea in children)
            {
                Console.WriteLine(childArea.Name);
            }

            OccupancyStateMachine occupancy = new OccupancyStateMachine();

            Console.WriteLine("Occupancy status: {0}", occupancy.Status);
            var result = occupancy.TryUpdateState(Trigger.SensorActivity);

            //Console.WriteLine("Attempt to trigger activity: {0}", result);
            //Console.WriteLine("Occupancy status now : {0}", occupancy.Status);

            // System.Threading.Thread.Sleep(10000);

            result = occupancy.TryUpdateState(Trigger.AlarmPartSet);
            result = occupancy.TryUpdateState(Trigger.AlarmFullSet);
            //Console.WriteLine("Attempt to part set alarm: {0}", result);
            //Console.WriteLine("Occupancy status: {0}", occupancy.Status);

            // result = occupancy.TryUpdateState(Trigger.AlarmUnset);
            //Console.WriteLine("Attemp to unset alarm: {0}", result);
            //Console.WriteLine("Occupancy status: {0}", occupancy.Status);

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
