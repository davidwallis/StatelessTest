using System;
using System.Collections.Generic;
using System.Linq;
using Stateless;

namespace StatelessTest
{

    /// <summary>
    /// Location Class, used for storing locations within the home or garden
    /// </summary>
    [Serializable]
    public class Location
    {
        /// <summary>
        /// The occupancy timer
        /// </summary>
        private readonly System.Timers.Timer _occupancyTimer;

        /// <summary>
        /// The state machine
        /// </summary>
        private readonly StateMachine<State, Trigger> _stateMachine;

        /// <summary>
        /// Prevents a default instance of the <see cref="Location"/> class from being created.
        /// </summary>
        private Location()
        {
            // parameterless constructor for serialization
        }

        /// <summary>
        /// Creates the state machine.
        /// </summary>
        /// <returns></returns>
        private StateMachine<State, Trigger> CreateStateMachine()
        {
            var stateMachine = new StateMachine<State, Trigger>(State.UnOccupied);

            // look how to inject this..
            //stateMachine.OnTransitioned(OnTransitionedAction);
            stateMachine.OnTransitioned((s) => { Console.WriteLine(); });

            stateMachine.Configure(State.UnOccupied)
                .Permit(Trigger.SensorActivity, State.Occupied)
                .PermitReentry(Trigger.AlarmFullSet);

            stateMachine.Configure(State.Occupied)
                .Permit(Trigger.AlarmFullSet, State.UnOccupied)
                .Permit(Trigger.AlarmPartSet, State.Asleep) // add time check
                .Permit(Trigger.OccupancyTimerExpires, State.UnOccupied)
                .PermitReentry(Trigger.SensorActivity)
                .OnEntry(() => { StartTimer(stateMachine, OccupancyTimeout); });

            stateMachine.Configure(State.Asleep)
                .SubstateOf(State.Occupied)
                .Permit(Trigger.AlarmUnset, State.Occupied);

            stateMachine.OnUnhandledTrigger((state, trigger) =>
            {
                Console.WriteLine("Unhandled: '{0}' state, '{1}' trigger!");
            });

            // Quick test to sanity check my logic
            string graph = stateMachine.ToDotGraph();
            Console.WriteLine(graph);

            return stateMachine;
        }

        /// <summary>
        /// Resets the timer.
        /// </summary>
        private void ResetTimer()
        {
            Console.WriteLine($"{Name} Occupancy timer restarting");
            _occupancyTimer.Stop();
            _occupancyTimer.Start();
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="occupancyTimeout">The occupancy timeout.</param>
        private void StartTimer(StateMachine<State, Trigger> stateMachine, TimeSpan occupancyTimeout)
        {
            // If the occupancy timer is allready running, restart it
            if (IsTimerRunning)
            {
                ResetTimer();
                return;
            }

            IsTimerRunning = true;

            // Configure the timer object
            _occupancyTimer.Interval = occupancyTimeout.TotalMilliseconds;
            _occupancyTimer.Elapsed += (sender, e) =>
            {
                _occupancyTimer.Stop();
                IsTimerRunning = false;

                Console.WriteLine($"{Name} Occupancy timer expired and removed");
                if (stateMachine.IsInState(State.Occupied))
                {
                    stateMachine.Fire(Trigger.OccupancyTimerExpires);
                }
            };
            _occupancyTimer.Start();
            Console.WriteLine($"{Name} Occupancy timer started");
        }

        /// <summary>
        /// Gets or sets the occupancy timeout.
        /// </summary>
        /// <value>
        /// The occupancy timeout.
        /// </value>
        public TimeSpan OccupancyTimeout { get; set; }

        /// <summary>
        /// Gets or sets the temperature.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public double Temperature { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public Location Parent { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the modules.
        /// </summary>
        /// <value>
        /// The modules.
        /// </value>
        public List<ModuleReference> Modules { get; set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public List<Location> Children { get; }

        /// <summary>
        /// Gets or sets the occupants.
        /// </summary>
        /// <value>
        /// The occupants.
        /// </value>
        public List<Person> Occupants { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is timer running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is timer running; otherwise, <c>false</c>.
        /// </value>
        public bool IsTimerRunning { get; set; }

        /// <summary>
        /// Gets the state of the occupancy.
        /// </summary>
        /// <value>
        /// The state of the occupancy.
        /// </value>
        public State OccupancyState => _stateMachine.State;

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public Location(Location parent)
        {
            Parent = parent;
            Children = new List<Location>();
            OccupancyTimeout = new TimeSpan(0, 0, 0, 5);
            parent?.Children.Add(this);
            _occupancyTimer = new System.Timers.Timer();
            _stateMachine = CreateStateMachine();
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        /// <value>
        /// All children.
        /// </value>
        public IEnumerable<Location> AllChildren => Children.Union(Children.SelectMany(child => child.AllChildren));

        /// <summary>
        /// Gets a value indicating whether this instance has occupied children.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has occupied children; otherwise, <c>false</c>.
        /// </value>
        public bool HasOccupiedChildren => AllChildren.Any(child => child.OccupancyState == State.Occupied);

        /// <summary>
        /// Tries the state of the update.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <returns></returns>
        public bool TryUpdateState(Trigger trigger)
        {
            if (!_stateMachine.CanFire(trigger))
                return false;

            _stateMachine.Fire(trigger);
            return true;
        }
    }
}
