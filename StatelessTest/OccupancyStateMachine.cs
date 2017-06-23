using Stateless;
using System;
using System.Reactive.Linq;

namespace StatelessTest
{
    public class OccupancyStateMachine
    {
        private readonly StateMachine<State, Trigger> _stateMachine;

        public OccupancyStateMachine()
        {
            _stateMachine = CreateStateMachine();
        }

        public bool TryUpdateState(Trigger trigger)
        {
            if (!_stateMachine.CanFire(trigger))
                return false;

            _stateMachine.Fire(trigger);
            return true;
        }

        public State Status => _stateMachine.State;

        private StateMachine<State, Trigger> CreateStateMachine()
        {
            var stateMachine = new StateMachine<State, Trigger>(State.UnOccupied);

            stateMachine.OnTransitioned(OnTransitionedAction);

            stateMachine.Configure(State.UnOccupied)
                .Permit(Trigger.SensorActivity, State.Occupied)
                .PermitReentry(Trigger.AlarmFullSet);

            stateMachine.Configure(State.Occupied)
                .Permit(Trigger.AlarmFullSet, State.UnOccupied)
                .Permit(Trigger.AlarmPartSet, State.Asleep)
                .Permit(Trigger.OccupancyTimerExpires, State.UnOccupied)
                .PermitReentry(Trigger.SensorActivity)
                .OnEntry(() =>
                {
                    Observable
                        .Timer(TimeSpan.FromSeconds(5))
                        .Subscribe(x =>
                        {
                            Console.WriteLine("Timer Expired");
                            if (stateMachine.IsInState(State.Occupied))
                            {
                                stateMachine.Fire(Trigger.OccupancyTimerExpires);
                            }
                        });
                });

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

        private static void OnTransitionedAction(StateMachine<State, Trigger>.Transition transition)
        {
            Console.WriteLine("Trigger: {0} Source: {1} dest: {2}", transition.Trigger, transition.Source, transition.Destination);
        }
    }
}
