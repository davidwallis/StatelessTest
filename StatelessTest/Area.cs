using System;
using System.Collections.Generic;
using System.Linq;

namespace StatelessTest
{
    public class Area
    {
        public TimeSpan OccupancyTimeout { get; set; }

        public Area Parent { get; }

        public string Name { get; set; }

        public List<Area> Children { get; }

        public bool IsTimerRunning { get; set; }

        public State OccupancyState { get; set; }

        public Area(Area parent)
        {
            Parent = parent;
            Children = new List<Area>();
            OccupancyTimeout = new TimeSpan(0, 0, 0, 5);
            parent?.Children.Add(this);
        }

        public IEnumerable<Area> AllChildren => Children.Union(Children.SelectMany(child => child.AllChildren));

        public bool HasOccupiedChildren => AllChildren.Any(child => child.OccupancyState == State.Occupied);
    }
}
