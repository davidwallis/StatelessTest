using Stateless;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace StatelessTest
{

    // https://stackoverflow.com/questions/6649983/how-to-let-a-parent-class-know-about-a-change-in-its-children

    // TODO:
    // use composite disposable for rx timer..
    // see https://sachabarbs.wordpress.com/2013/05/16/simple-but-nice-state-machine/


    /// <summary>
    /// Location Class, used for storing locations within the home or garden
    /// </summary>
    [Serializable]
    public class Location
    {

        //[OnDeserializing()]
        //internal void OnDeserializingMethod(StreamingContext context)
        //{
        //    // System.Console.WriteLine("SerRoot.OnDeserializingMethod");
        //}

        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // System.Console.WriteLine("SerRoot.OnDeserializedMethod");
            this._occupancyTimer = new System.Timers.Timer();
            this._stateMachine = this.CreateStateMachine(this.OccupancyState);
        }

        //[OnSerializing()]
        //internal void OnSerializingMethod(StreamingContext context)
        //{
        //    System.Console.WriteLine("SerRoot.OnSerializingMethod");
        //}

        //[OnSerialized()]
        //internal void OnSerializedMethod(StreamingContext context)
        //{
        //    System.Console.WriteLine("SerRoot.OnSerializedMethod");
        //}

        /// <summary>
        /// The occupancy timer
        /// </summary>
        [NonSerialized]
        private System.Timers.Timer _occupancyTimer;

        /// <summary>
        /// The state machine
        /// </summary>
        [NonSerialized]
        private StateMachine<State, Trigger> _stateMachine;

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
        // TODO look how to inject this..
        private StateMachine<State, Trigger> CreateStateMachine(State initialState = State.UnOccupied)
        {
            var stateMachine = new StateMachine<State, Trigger>(initialState);

            // stateMachine.OnTransitioned(OnTransitionedAction);

            stateMachine.Configure(State.UnOccupied)
                .Permit(Trigger.SensorActivity, State.Occupied)
                .Permit(Trigger.ChildOccupied, State.ChildOccupied)
                .PermitReentry(Trigger.AlarmFullSet);

            stateMachine.Configure(State.Occupied)
                .Permit(Trigger.AlarmFullSet, State.UnOccupied)
                .Permit(Trigger.AlarmPartSet, State.Asleep) // add check for which part set (IE dogs or Bed)
                .Permit(Trigger.OccupancyTimerExpires, State.UnOccupied)
                .PermitReentry(Trigger.SensorActivity)
                .OnEntry(() =>
                {
                    StartTimer(stateMachine, OccupancyTimeout);
                    if (Parent == null) return;
                    if (Parent.TryUpdateState(Trigger.ChildOccupied))
                    {
                        Console.WriteLine($"Child [{Name}] Occupied, setting parent [{Parent.Name}] state to ChildOccupied");
                    }
                    else
                    { 
                        Console.WriteLine("Unable to update child state");
                    }
                });

            stateMachine.Configure(State.ChildOccupied)
                .SubstateOf(State.Occupied)
                .PermitReentry(Trigger.ChildOccupied);

            stateMachine.Configure(State.Asleep)
                .SubstateOf(State.Occupied)
                .Permit(Trigger.AlarmUnset, State.Occupied);

            stateMachine.OnUnhandledTrigger((state, trigger) =>
            {
                Console.WriteLine("Unhandled: '{0}' state, '{1}' trigger!");
            });

            // Quick test to sanity check my logic
            //Console.WriteLine(stateMachine.ToDotGraph());

            return stateMachine;
        }

        private void OnTransitionedAction(StateMachine<State, Trigger>.Transition transition)
        {
            // if its the top level state, there will be no parent.
            if (Parent == null) return;


            // Determine the state being transitioned to
            OccupancyState = transition.Destination;

            // If the child state isn't occupped or child occupied then ignore the transition
            // Should I be passing _stateMachine in to avoid the dependency within the method - if so how?
            if (!_stateMachine.IsInState(State.Occupied)) return;

            // previous way of testing
            // if (OccupancyState != State.Occupied && OccupancyState != State.ChildOccupied) return;

            Console.WriteLine($"Child [{Name}] Occupied, setting parent [{Parent.Name}] state to ChildOccupied");

            if (!Parent.TryUpdateState(Trigger.ChildOccupied))
            {
                Console.WriteLine("Unable to update child state");
            }

        }

        private State _state;
        private TimeSpan m_OccupancyTimeout;

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
                    Console.WriteLine("{0} in state {1} - Firing OccupancyTimerExpires", Name, stateMachine.State);
                    stateMachine.Fire(Trigger.OccupancyTimerExpires);
                }
            };
            _occupancyTimer.Start();
            Console.WriteLine($"{Name} Occupancy timer started");
        }

        ///// <summary>
        ///// Gets or sets the occupancy timeout.
        ///// </summary>
        ///// <value>
        ///// The occupancy timeout.
        ///// </value>
        [XmlIgnore]
        public TimeSpan OccupancyTimeout
        {
            get => this.m_OccupancyTimeout;
            set => this.m_OccupancyTimeout = value;
        }

        /// <summary>
        /// Gets or sets the string representation of the timeSpan, XMLSerialisation doesnt support Timespan, so use this property for serialization instead.
        /// </summary>
        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        //[XmlElement(DataType = "duration", ElementName = "OccupancyTimeout")]
        public string OccupancyTimeoutString
        {
            get => XmlConvert.ToString(this.OccupancyTimeout);
            set => this.OccupancyTimeout = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
        }

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
        public List<Location> Children { get; set; }

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
        public State OccupancyState
        {
            get => _state;
            set => _state = value;
        }
        //=> _stateMachine.State;

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public Location(Location parent)
        {
            Parent = parent;
            //_children = new ObservableCollection<Location>();
            Children = new List<Location>();
            //  _children.CollectionChanged += CollectionChanged;
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


        /// <summary>
        /// The reason the state transitioned
        /// TODO testing - this might not be practical
        /// </summary>
        public string TransitionReason { get; set; }
    }
}
