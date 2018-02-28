using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatelessTest
{
    class GenerateAndSerialize
    {
        public static void GenerateAndSerializeData()
        {
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
            SerializationHelper.BinarySerialize(locations);

            locations = null;
        }
    }
}
