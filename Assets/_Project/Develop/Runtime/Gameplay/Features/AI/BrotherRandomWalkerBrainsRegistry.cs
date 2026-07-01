using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Utilities.Timer;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI
{
    public class BrotherRandomWalkerBrainHandle
    {
        public BrotherRandomWalkerBrainHandle(
            AIStateMachine stateMachine,
            RandomMovementState movementState,
            InformativeIdleState idleState,
            TimerService movementTimer,
            TimerService idleTimer)
        {
            StateMachine = stateMachine;
            MovementState = movementState;
            IdleState = idleState;
            MovementTimer = movementTimer;
            IdleTimer = idleTimer;
        }

        public AIStateMachine StateMachine { get; }

        public RandomMovementState MovementState { get; }

        public InformativeIdleState IdleState { get; }

        public TimerService MovementTimer { get; }

        public TimerService IdleTimer { get; }

        public void RestartIdleCycle()
        {
            MovementTimer.Restart();
            IdleTimer.Restart();
            StateMachine.ForceState(IdleState);
        }
    }

    public class BrotherRandomWalkerBrainsRegistry
    {
        private readonly Dictionary<Entity, BrotherRandomWalkerBrainHandle> _handles = new();

        public void Register(Entity entity, BrotherRandomWalkerBrainHandle handle)
        {
            _handles[entity] = handle;
        }

        public bool TryGetHandle(Entity entity, out BrotherRandomWalkerBrainHandle handle)
        {
            return _handles.TryGetValue(entity, out handle);
        }

        public bool TryRestartIdleCycle(Entity entity)
        {
            if (_handles.TryGetValue(entity, out BrotherRandomWalkerBrainHandle handle) == false)
                return false;

            handle.RestartIdleCycle();
            return true;
        }
    }
}
