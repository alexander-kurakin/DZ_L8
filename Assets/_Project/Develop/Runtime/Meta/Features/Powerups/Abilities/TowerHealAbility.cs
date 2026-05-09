using System;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;

namespace _Project.Develop.Runtime.Meta.Features.Powerups.Abilities
{
    public class TowerHealAbility : Powerup, IDisposable
    {
        private Entity _mainHero;
        private PermanentTowerHealConfig _permanentTowerHealConfig;

        private IDisposable _gameplayStateDisposable;
        
        public TowerHealAbility(
            Entity mainHero,
            PermanentTowerHealConfig permanentTowerHealConfig,
            int currentLevel) : base(permanentTowerHealConfig.ID, currentLevel, permanentTowerHealConfig.MaxLevel)
        {
            _mainHero = mainHero;
            _permanentTowerHealConfig = permanentTowerHealConfig;
        }

        public override void Activate()
        {
            _gameplayStateDisposable = _mainHero.GameplayPhase.Subscribe(OnGameplayPhaseChanged);
        }

        private void OnGameplayPhaseChanged(GameplayStates oldState, GameplayStates newState)
        {
            if (oldState == GameplayStates.Preparation && newState == GameplayStates.StageProcess)
                Heal();
        }

        private void Heal()
        {
            float maxHealth = _mainHero.MaxHealth.Value;
            float currentHealth = _mainHero.CurrentHealth.Value;

            float newCurrentHealth = MathF.Min(maxHealth, currentHealth + maxHealth * _permanentTowerHealConfig.PercentOfTowerLifeHealed);
            _mainHero.CurrentHealth.Value = newCurrentHealth;
        }

        public void Dispose()
        {
            _gameplayStateDisposable?.Dispose();
        }
    }
}