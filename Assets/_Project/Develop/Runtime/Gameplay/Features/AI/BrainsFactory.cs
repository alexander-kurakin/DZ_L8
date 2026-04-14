using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.Timer;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI
{
    public class BrainsFactory
    {
        private readonly DIContainer _container;
        private readonly AIBrainsContext _brainsContext;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        public BrainsFactory(DIContainer container)
        {
            _container = container;
            _brainsContext = _container.Resolve<AIBrainsContext>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
        }

        public StateMachineBrain CreateWalkingEnemyBrain(Entity entity, ITargetSelector targetSelector)
        {
            FindTargetState findTargetState = new FindTargetState(targetSelector, _entitiesLifeContext, entity);
            MoveToTargetState moveToTargetState = new MoveToTargetState(entity);
            RotateToTargetState rotateToTargetState = new RotateToTargetState(entity);
            
            AIParallelState chaseState = new AIParallelState(moveToTargetState, rotateToTargetState);
            
            ReactiveVariable<Entity> currentTarget = entity.CurrentTarget;
            ICondition canMove = entity.CanMove;
            
            ICompositeCondition fromFindToChaseCondition = new CompositeCondition()
                .Add(canMove)
                .Add(new FuncCondition(() => currentTarget.Value != null));
            
            AIStateMachine rootStateMachine = new AIStateMachine();
            
            rootStateMachine.AddState(findTargetState);
            rootStateMachine.AddState(chaseState);
            
            rootStateMachine.AddTransition(findTargetState, chaseState, fromFindToChaseCondition);
            
            StateMachineBrain brain = new StateMachineBrain(rootStateMachine);
            _brainsContext.SetFor(entity,brain);
            
            return brain;
        }
        
        public StateMachineBrain CreateTowerWalkerBrain(Entity entity, MouseInput mouseInput)
        {
            MovingTowardsCursorState state = new MovingTowardsCursorState(entity, mouseInput);
            
            AIStateMachine stateMachine = new AIStateMachine();
            
            stateMachine.AddState(state);
            
            StateMachineBrain brain = new StateMachineBrain(stateMachine);
            _brainsContext.SetFor(entity, brain);
            
            return brain;
        }
    }
}
