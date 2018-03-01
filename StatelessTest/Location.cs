using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Stateless;

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
            this._occupancyTimer = new System.Timers.Timer();
            this.stateMachine = this.CreateStateMachine(this.OccupancyState);
            this.ResumeTimer(this.stateMachine, this.OccupancyTimeout);
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
        private StateMachine<State, Trigger> stateMachine;

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
            this.stateMachine = new StateMachine<State, Trigger>(initialState);

            // stateMachine.OnTransitioned(OnTransitionedAction);

            this.stateMachine.Configure(State.UnOccupied)
                .Permit(Trigger.SensorActivity, State.Occupied)
                .Permit(Trigger.ChildOccupied, State.ChildOccupied)
                .PermitReentry(Trigger.AlarmFullSet)
                .OnEntry(this.UnOccupiedOnEntry);

            this.stateMachine.Configure(State.Occupied)
                .Permit(Trigger.AlarmFullSet, State.UnOccupied)
                .Permit(Trigger.AlarmPartSet, State.Asleep) // add check for which part set (IE dogs or Bed)
                .Permit(Trigger.OccupancyTimerExpires, State.UnOccupied)
                .PermitReentry(Trigger.SensorActivity)
                // .Ignore(Trigger.ChildOccupied)
                .OnEntry(this.OccupiedOnEntry);

            this.stateMachine.Configure(State.ChildOccupied)
                //.SubstateOf(State.Occupied)
                .PermitReentry(Trigger.ChildOccupied)
                .Permit(Trigger.SensorActivity, State.Occupied)
                .OnEntry(this.ChildOccupiedOnEntry);

            this.stateMachine.Configure(State.Asleep)
                .SubstateOf(State.Occupied)
                .Permit(Trigger.AlarmUnset, State.Occupied)
                .OnEntry(this.AsleepOnEntry);

            this.stateMachine.OnUnhandledTrigger((state, trigger) =>
            {
                Console.WriteLine($"Unhandled: '{state}' state, '{trigger}' trigger!");
            });

            return this.stateMachine;
        }

        private void UnOccupiedOnEntry()
        {
            Console.WriteLine($"[{this.Name}] UnOccupied, Turn Lights Off");
        }

        private void AsleepOnEntry()
        {
            Console.WriteLine($"{this.Name} asleep"); 
        }

        private void OccupiedOnEntry()
        {
            Console.WriteLine($"[{this.Name}] [{this.stateMachine.State}] Occupied, Turn Lights On");
            this.StartTimer(this.stateMachine, this.OccupancyTimeout);

            if (this.Parent?.TryUpdateState(Trigger.ChildOccupied) == true)
            {
                Console.WriteLine($"[{this.Name}] Occupied, setting parent [{this.Parent.Name}] state to ChildOccupied");
            }
        }

        private void ChildOccupiedOnEntry()
        {
            // Console.WriteLine($"{Name} has an occupied child");
            if (this.Parent?.TryUpdateState(Trigger.ChildOccupied) == true)
            {
                Console.WriteLine($"[{this.Name}] ChildOccupied, setting parent [{this.Parent.Name}] state to ChildOccupied");
            }
        }

        //private void OnTransitionedAction(StateMachine<State, Trigger>.Transition transition)
        //{
        //    // if its the top level state, there will be no parent.
        //    if (Parent == null) return;

        //    // Determine the state being transitioned to
        //    //OccupancyState = transition.Destination;

        //    // If the child state isn't occupped or child occupied then ignore the transition
        //    // Should I be passing _stateMachine in to avoid the dependency within the method - if so how?
        //    if (!stateMachine.IsInState(State.Occupied)) return;

        //    // previous way of testing
        //    // if (OccupancyState != State.Occupied && OccupancyState != State.ChildOccupied) return;

        //    Console.WriteLine($"Child [{Name}] Occupied, setting parent [{Parent.Name}] state to ChildOccupied");

        //    if (!Parent.TryUpdateState(Trigger.ChildOccupied))
        //    {
        //        Console.WriteLine("Unable to update child state");
        //    }
        //}

        private TimeSpan m_OccupancyTimeout;

        /// <summary>
        /// Resets the timer.
        /// </summary>
        private void ResetTimer()
        {
            Console.WriteLine($"{this.Name} Occupancy timer restarting");
            this._occupancyTimer.Stop();
            this._occupancyTimer.Start();
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="occupancyTimeout">The occupancy timeout.</param>
        private void StartTimer(StateMachine<State, Trigger> sm, TimeSpan occupancyTimeout)
        {
            // If the occupancy timer is already running, restart it
            if (this.IsTimerRunning)
            {
                this.ResetTimer();
                return;
            }

            this.IsTimerRunning = true;

            // Configure the timer object
            this._occupancyTimer.Interval = occupancyTimeout.TotalMilliseconds;
            this._occupancyTimer.Elapsed += (sender, e) =>
            {
                this._occupancyTimer.Stop();
                this.IsTimerRunning = false;

                if (sm.IsInState(State.Occupied))
                {
                    //Console.WriteLine("{0} in state {1} - Firing OccupancyTimerExpires", Name, stateMachine.State);
                    sm.Fire(Trigger.OccupancyTimerExpires);
                }
            };
            this._occupancyTimer.Start();
            Console.WriteLine($"{this.Name} Occupancy timer started");
        }

        private void ResumeTimer(StateMachine<State, Trigger> stateMachine, TimeSpan occupancyTimeout)
        {
            // was the occupancy timer already running? restart it
            if (!this.IsTimerRunning || this._occupancyTimer.Enabled ) { return; }

            Console.WriteLine($"Resuming timer in [{this.Name}] for {occupancyTimeout.TotalSeconds} Seconds");

            // Configure the timer object
            this._occupancyTimer.Interval = occupancyTimeout.TotalMilliseconds;
            this._occupancyTimer.Elapsed += (sender, e) =>
            {
                this._occupancyTimer.Stop();
                this.IsTimerRunning = false;

                Console.WriteLine($"{this.Name} Occupancy timer expired and removed");
                if (stateMachine.IsInState(State.Occupied))
                {
                    Console.WriteLine("{0} in state {1} - Firing OccupancyTimerExpires", this.Name, stateMachine.State);
                    stateMachine.Fire(Trigger.OccupancyTimerExpires);
                }
            };
            this._occupancyTimer.Start();
            Console.WriteLine($"{this.Name} Occupancy timer started");
        }

        /////// <summary>
        /////// Gets or sets the occupancy timeout.
        /////// </summary>
        /////// <value>
        /////// The occupancy timeout.
        /////// </value>
        //[XmlIgnore]
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
        public State OccupancyState => this.stateMachine?.State ?? State.UnOccupied;

        /// <summary>
        /// Gets the state of the occupancy.
        /// </summary>
        /// <value>
        /// The state of the occupancy.
        /// </value>
        //public State OccupancyState
        //{
        //    get => _state;
        //    set => _state = value;
        //}

        //public State OccupancyState => stateMachine.State;

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public Location(Location parent)
        {
            this.Parent = parent;
            //_children = new ObservableCollection<Location>();
            this.Children = new List<Location>();

            //  _children.CollectionChanged += CollectionChanged;
            this.OccupancyTimeout = new TimeSpan(0, 0, 0, 5);

            parent?.Children.Add(this);

            this._occupancyTimer = new System.Timers.Timer();
            this.stateMachine = this.CreateStateMachine();
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        /// <value>
        /// All children.
        /// </value>
        public IEnumerable<Location> AllChildren => this.Children.Union(this.Children.SelectMany(child => child.AllChildren));

        /// <summary>
        /// Gets a value indicating whether this instance has occupied children.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has occupied children; otherwise, <c>false</c>.
        /// </value>
        public bool HasOccupiedChildren => this.AllChildren.Any(child => child.OccupancyState == State.Occupied);

        /// <summary>
        /// Tries the state of the update.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <returns></returns>
        public bool TryUpdateState(Trigger trigger)
        {
            if (!this.stateMachine.CanFire(trigger))
            {
                Console.WriteLine($"Unable to firetrigger {trigger}");
                return false;
            }

            this.stateMachine.Fire(trigger);
            return true;
        }


        /// <summary>
        /// The reason the state transitioned
        /// TODO testing - this might not be practical
        /// </summary>
        public string TransitionReason { get; set; }

        /// <summary>
        /// Generates a dotgraph representation of the state machine
        /// </summary>
        public string StateMachineAsDotGraph => this.stateMachine.ToDotGraph();
    }
}
