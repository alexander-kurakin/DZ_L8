using Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
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
        private readonly TimerServiceFactory _timerServiceFactory;
        private readonly AIBrainsContext _brainsContext;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        public BrainsFactory(DIContainer container)
        {
            _container = container;
            _brainsContext = _container.Resolve<AIBrainsContext>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _timerServiceFactory = _container.Resolve<TimerServiceFactory>();
        }
        
        public StateMachineBrain CreateSimpleMainHeroBrain(Entity entity, IInputService desktopInput)
        {
            AIStateMachine stateMachine = CreatePlayerMovementStateMachine(entity, desktopInput);
            StateMachineBrain brain = new StateMachineBrain(stateMachine);

            _brainsContext.SetFor(entity, brain);

            return brain;
        }

        private AIStateMachine CreatePlayerMovementStateMachine(Entity entity, IInputService desktopInput)
        {
            IMouseInputService mouseInput = _container.Resolve<IMouseInputService>();
            IMouseRaycastService mouseRaycastService = _container.Resolve<IMouseRaycastService>();

            FaceMousePointerState faceMousePointerState = new FaceMousePointerState(entity, mouseInput, mouseRaycastService);
            PlayerInputMovementState playerInputMovementState = new PlayerInputMovementState(
                entity,
                desktopInput,
                mouseInput,
                mouseRaycastService);

            AIStateMachine stateMachine = new AIStateMachine();

            stateMachine.AddState(faceMousePointerState);
            stateMachine.AddState(playerInputMovementState);

            stateMachine.AddTransition(
                faceMousePointerState,
                playerInputMovementState,
                new FuncCondition(() => desktopInput.Direction != Vector3.zero));

            stateMachine.AddTransition(
                playerInputMovementState,
                faceMousePointerState,
                new FuncCondition(() => desktopInput.Direction == Vector3.zero));

            return stateMachine;
        }

        public StateMachineBrain CreateRotatingAutoAttackBrain(Entity entity, ITargetSelector targetSelector)
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

            //ICondition canAttack = entity.CanStartAttack; --attack components
            Transform transform = entity.Transform;
            ReactiveVariable<Entity> currentTarget = entity.CurrentTarget;

            //ICompositeCondition fromRotateToAttackCondition = new CompositeCondition()
            //    .Add(canAttack)
                //.Add(new FuncCondition(() =>
                //{
                   // Entity target = currentTarget.Value;

                   // if (target == null)
                   //     return false;

                    //float angleToTarget = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(target.Transform.position - transform.position));
                   // return angleToTarget < 3f;
               // }));

          //  ReactiveVariable<bool> inAttackProcess = entity.InAttackProcess;

            //ICondition fromAttackToRotateStateCondition = new FuncCondition(() => inAttackProcess.Value == false);

            AIStateMachine stateMachine = new AIStateMachine();

            stateMachine.AddState(rotateToTargetState);
            stateMachine.AddState(attackTriggerState);

           // stateMachine.AddTransition(rotateToTargetState, attackTriggerState, fromRotateToAttackCondition);
          //  stateMachine.AddTransition(attackTriggerState, rotateToTargetState, fromAttackToRotateStateCondition);

            return stateMachine;
        }

        public StateMachineBrain CreateGnomePeekBrain(Entity entity, GnomeConfig config)
        {
            MainHeroHolderService mainHeroHolderService = _container.Resolve<MainHeroHolderService>();

            GnomeHiddenState hiddenState = new GnomeHiddenState(entity, config);
            GnomePeekingState peekingState = new GnomePeekingState(entity, config, mainHeroHolderService);

            AIStateMachine stateMachine = new AIStateMachine();
            stateMachine.AddState(hiddenState);
            stateMachine.AddState(peekingState);

            stateMachine.AddTransition(
                hiddenState,
                peekingState,
                new FuncCondition(() => hiddenState.ShouldPeek));

            stateMachine.AddTransition(
                peekingState,
                hiddenState,
                new FuncCondition(() => peekingState.ShouldHide));

            StateMachineBrain brain = new StateMachineBrain(stateMachine);
            _brainsContext.SetFor(entity, brain);

            return brain;
        }
    }
}
