using System;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;
using Random = UnityEngine.Random;

namespace _Project.Develop.Runtime.Meta.Features.Powerups.Abilities
{
    public class LuckyRestoreAbility : Powerup, IDisposable
    {
        private readonly Entity _mainHero;
        private readonly PermanentLuckyRestoreConfig _config;

        private IDisposable _gameplayStateDisposable;

        public LuckyRestoreAbility(
            Entity mainHero,
            PermanentLuckyRestoreConfig config,
            int currentLevel) : base(config.ID, currentLevel, config.MaxLevel)
        {
            _mainHero = mainHero;
            _config = config;
        }

        public override void Activate()
        {
            _gameplayStateDisposable = _mainHero.GameplayPhase.Subscribe(OnGameplayPhaseChanged);
        }

        private void OnGameplayPhaseChanged(GameplayStates oldState, GameplayStates newState)
        {
            if (oldState == GameplayStates.Preparation && newState == GameplayStates.StageProcess)
                RestoreIntegrity();
        }

        private void RestoreIntegrity()
        {
            float maxIntegrity = _mainHero.MaxHealth.Value;
            float currentIntegrity = _mainHero.CurrentHealth.Value;
            float missingIntegrity = maxIntegrity - currentIntegrity;

            if (missingIntegrity <= 0f)
                return;

            float restoreFraction = Random.Range(
                _config.MinMissingIntegrityRestoreFraction,
                _config.MaxMissingIntegrityRestoreFraction);
            float restoredAmount = missingIntegrity * restoreFraction;
            float newIntegrity = MathF.Min(maxIntegrity, currentIntegrity + restoredAmount);
            _mainHero.CurrentHealth.Value = newIntegrity;
        }

        public void Dispose()
        {
            _gameplayStateDisposable?.Dispose();
        }
    }
}
