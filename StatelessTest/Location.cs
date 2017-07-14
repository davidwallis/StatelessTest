using Stateless;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace StatelessTest
{
    //TODO:
    // https://stackoverflow.com/questions/6649983/how-to-let-a-parent-class-know-about-a-change-in-its-children

    // TODO:
    // use composite disposable for rx timer..
    // see https://sachabarbs.wordpress.com/2013/05/16/simple-but-nice-state-machine/


    /// <summary>
    /// Location Class, used for storing locations within the home or garden
    /// </summary>
    [Serializable]
    public class Location: INotifyPropertyChanged
    {
        /// <summary>
        /// The occupancy timer
        /// </summary>
        private readonly System.Timers.Timer _occupancyTimer;

        /// <summary>
        /// The state machine
        /// </summary>
        private readonly StateMachine<State, Trigger> _stateMachine;
    
        private readonly ObservableCollection<Location> _children;

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
        private StateMachine<State, Trigger> CreateStateMachine()
        {
            var stateMachine = new StateMachine<State, Trigger>(State.UnOccupied);

            stateMachine.OnTransitioned(OnTransitionedAction);

            stateMachine.Configure(State.UnOccupied)
                .Permit(Trigger.SensorActivity, State.Occupied)
                .Permit(Trigger.ChildOccupied, State.ChildOccupied)
                .PermitReentry(Trigger.AlarmFullSet);

            stateMachine.Configure(State.Occupied)
                .Permit(Trigger.AlarmFullSet, State.UnOccupied)
                .Permit(Trigger.AlarmPartSet, State.Asleep) // add time check
                .Permit(Trigger.OccupancyTimerExpires, State.UnOccupied)
                .PermitReentry(Trigger.SensorActivity)
                .OnEntry(() => { StartTimer(stateMachine, OccupancyTimeout); });

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
            //string graph = stateMachine.ToDotGraph();
            //Console.WriteLine(graph);

            return stateMachine;
        }

        private void OnTransitionedAction(StateMachine<State, Trigger>.Transition transition)
        {
            // if its the top level state, there will be no parent.
            if (Parent == null) return;


            // Determine the state being transitioned to
            OccupancyState = transition.Destination;

            // If the child state isn't occupped or child occupied then ignore the transition
            if (OccupancyState != State.Occupied && OccupancyState != State.ChildOccupied) return;

            
            Console.WriteLine($"Child [{Name}] Occupied, setting parent [{Parent.Name}] state to ChildOccupied");

            if (!Parent.TryUpdateState(Trigger.ChildOccupied))
            { 
                Console.WriteLine("Unable to update child state");
            }

        }

        private State _state;

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
        public ObservableCollection<Location> Children => _children;

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
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
            
        }
        //=> _stateMachine.State;

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public Location(Location parent)
        {
            Parent = parent;
            _children = new ObservableCollection<Location>();
            _children.CollectionChanged += CollectionChanged;
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
        
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Location item in e.NewItems)
                    item.PropertyChanged += Location_PropertyChanged;

            if (e.OldItems != null)
                foreach (Location item in e.OldItems)
                    item.PropertyChanged -= Location_PropertyChanged;
        }

        /// <summary>
        /// The reason the state transitioned
        /// TODO testing - this might not be practical
        /// </summary>
        public string TransitionReason { get; set; }

        void Location_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
          
            //Console.WriteLine($"Property Name: {e.PropertyName}");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
