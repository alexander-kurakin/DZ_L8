using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.Timer;
using UnityEngine;

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
        
        public StateMachineBrain CreateRangedEnemyBrain(Entity entity, ITargetSelector targetSelector)
        {
            FindTargetState findTargetState = new FindTargetState(targetSelector, _entitiesLifeContext, entity);
            
            MoveToTargetState moveToTargetState = new MoveToTargetState(entity);
            RotateToTargetState rotateToTargetState = new RotateToTargetState(entity);
            
            AIParallelState chaseState = new AIParallelState(moveToTargetState, rotateToTargetState);
            
            AIStateMachine autoAttackState = CreateAutoAttackStateMachine(entity);
            
            AIStateMachine rootStateMachine = new AIStateMachine();
            rootStateMachine.AddState(findTargetState);
            rootStateMachine.AddState(chaseState);
            rootStateMachine.AddState(autoAttackState);
            
            ReactiveVariable<Entity> currentTarget = entity.CurrentTarget;
            ReactiveVariable<bool> distanceToTargetReached = entity.DistanceToTargetReached;
            ICondition canMove = entity.CanMove;
            
            ICompositeCondition fromFindToChaseCondition = new CompositeCondition()
                .Add(canMove)
                .Add(new FuncCondition(() => currentTarget.Value != null));
            
            ICompositeCondition fromChaseToAutoAttackCondition = new CompositeCondition()
                .Add(new FuncCondition(() => currentTarget.Value != null))
                .Add(new FuncCondition(() => distanceToTargetReached.Value));

            //if current target is null = game over

            rootStateMachine.AddTransition(findTargetState, chaseState, fromFindToChaseCondition);
            rootStateMachine.AddTransition(chaseState, autoAttackState, fromChaseToAutoAttackCondition);
            
            StateMachineBrain brain = new StateMachineBrain(rootStateMachine);
            
            _brainsContext.SetFor(entity, brain);
            
            return brain;
        }

        public StateMachineBrain CreateTurretBrain(Entity entity, ITargetSelector targetSelector)
        {
            FindTargetState findTargetState = new FindTargetState(targetSelector, _entitiesLifeContext, entity);
            AIStateMachine autoAttackState = CreateAutoAttackStateMachine(entity);

            AIStateMachine rootStateMachine = new AIStateMachine();
            rootStateMachine.AddState(findTargetState);
            rootStateMachine.AddState(autoAttackState);
            
            ReactiveVariable<Entity> currentTarget = entity.CurrentTarget;
            
            ICompositeCondition fromFindToAutoAttackCondition = new CompositeCondition()
                .Add(new FuncCondition(() => currentTarget.Value != null));

            rootStateMachine.AddTransition(findTargetState, autoAttackState, fromFindToAutoAttackCondition);
            
            ICompositeCondition fromAutoAttackToFindCondition = new CompositeCondition()
                .Add(new FuncCondition(() => currentTarget.Value == null));

            rootStateMachine.AddTransition(autoAttackState, findTargetState, fromAutoAttackToFindCondition);
            
            StateMachineBrain brain = new StateMachineBrain(rootStateMachine);
            
            _brainsContext.SetFor(entity, brain);
            
            return brain;
        }

        private AIStateMachine CreateAutoAttackStateMachine(Entity entity)
        {
            RotateToTargetState rotateToTargetState = new RotateToTargetState(entity);

            AttackTriggerState attackTriggerState = new AttackTriggerState(entity);

            ICondition canAttack = entity.CanStartAttack;
            Transform transform = entity.Transform;
            ReactiveVariable<Entity> currentTarget = entity.CurrentTarget;

            ICompositeCondition fromRotateToAttackCondition = new CompositeCondition()
                .Add(canAttack)
                .Add(new FuncCondition(() =>
                {
                    Entity target = currentTarget.Value;

                    if (target == null)
                        return false;

                    float angleToTarget = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(target.Transform.position - transform.position));
                    return angleToTarget < 3f;
                }));

            ReactiveVariable<bool> inAttackProcess = entity.InAttackProcess;

            ICondition fromAttackToRotateStateCondition = new FuncCondition(() => inAttackProcess.Value == false);

            AIStateMachine stateMachine = new AIStateMachine();

            stateMachine.AddState(rotateToTargetState);
            stateMachine.AddState(attackTriggerState);

            stateMachine.AddTransition(rotateToTargetState, attackTriggerState, fromRotateToAttackCondition);
            stateMachine.AddTransition(attackTriggerState, rotateToTargetState, fromAttackToRotateStateCondition);

            return stateMachine;
        }        
        
    }
}
