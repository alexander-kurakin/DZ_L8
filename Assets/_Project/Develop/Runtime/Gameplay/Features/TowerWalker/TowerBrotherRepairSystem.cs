using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public class TowerBrotherRepairSystem : IInitializableSystem
    {
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly SpellcoreCombatConfig _spellcoreCombatConfig;
        private readonly BrotherRandomWalkerBrainsRegistry _brotherRandomWalkerBrainsRegistry;

        private ReactiveVariable<GameplayStates> _gameplayPhase;
        private IDisposable _movementPhaseEnteredSubscription;

        public TowerBrotherRepairSystem(
            MainHeroHolderService mainHeroHolderService,
            SpellcoreCombatConfig spellcoreCombatConfig,
            BrotherRandomWalkerBrainsRegistry brotherRandomWalkerBrainsRegistry)
        {
            _mainHeroHolderService = mainHeroHolderService;
            _spellcoreCombatConfig = spellcoreCombatConfig;
            _brotherRandomWalkerBrainsRegistry = brotherRandomWalkerBrainsRegistry;
        }

        public void OnInit(Entity entity)
        {
            _gameplayPhase = entity.GameplayPhase;

            if (_brotherRandomWalkerBrainsRegistry.TryGetHandle(entity, out BrotherRandomWalkerBrainHandle brainHandle) == false)
                return;

            _movementPhaseEnteredSubscription = brainHandle.MovementState.Entered.Subscribe(OnMovementPhaseEntered);
        }

        private void OnMovementPhaseEntered()
        {
            if (_gameplayPhase.Value != GameplayStates.StageProcess)
                return;

            Entity tower = _mainHeroHolderService.MainHero;

            if (tower == null || tower.IsDead.Value)
                return;

            if (tower.CurrentHealth.Value >= tower.MaxHealth.Value)
                return;

            float hitsToRestore = _spellcoreCombatConfig.BrotherRepairHitsPerMovementPhase;
            float maxHealth = tower.MaxHealth.Value;
            float newHealth = tower.CurrentHealth.Value + hitsToRestore;

            if (newHealth > maxHealth)
                newHealth = maxHealth;

            tower.CurrentHealth.Value = newHealth;
        }
    }
}
