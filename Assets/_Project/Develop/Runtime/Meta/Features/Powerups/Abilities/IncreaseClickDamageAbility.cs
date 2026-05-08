using System;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace _Project.Develop.Runtime.Meta.Features.Powerups.Abilities
{
    public class IncreaseClickDamageAbility : Powerup, IDisposable
    {
        private readonly Entity _mainHero;
        private readonly PermanentIncreaseNormalAbilityDamageConfig _config;
        
        private IDisposable _phaseChangedDisposable;
        private ReactiveVariable<float> _explodeDamage;
        
        private float _baseDamage;
        private bool _isMultiplierApplied;
        
        public IncreaseClickDamageAbility(
            Entity mainHero,
            PermanentIncreaseNormalAbilityDamageConfig config,
            int currentLevel) : base(config.ID, currentLevel, config.MaxLevel)
        {
            _mainHero = mainHero;
            _config = config;
        }
        
        public override void Activate()
        {
            if (_mainHero.AbilityUserAllAbilities.TryGetValue(AbilityType.ExplodeAtPoint, out Entity explodeAbility) == false)
                return;
            
            if (explodeAbility.TryGetAreaImpactDamage(out ReactiveVariable<float> explodeDamage) == false)
                return;
            
            _explodeDamage = explodeDamage;
            _baseDamage = _explodeDamage.Value;
            
            _phaseChangedDisposable = _mainHero.GameplayPhase.Subscribe(OnGameplayPhaseChanged);
            
            if (_mainHero.GameplayPhase.Value == GameplayStates.StageProcess)
                ApplyMultiplier();
        }
        
        private void OnGameplayPhaseChanged(GameplayStates oldState, GameplayStates newState)
        {
            if (newState == GameplayStates.StageProcess)
                ApplyMultiplier();
            else if (oldState == GameplayStates.StageProcess)
                RemoveMultiplier();
        }
        
        private void ApplyMultiplier()
        {
            if (_isMultiplierApplied)
                return;
            
            _explodeDamage.Value = _baseDamage * _config.DamageMultiplier;
            _isMultiplierApplied = true;
        }
        
        private void RemoveMultiplier()
        {
            if (_isMultiplierApplied == false)
                return;
            
            _explodeDamage.Value = _baseDamage;
            _isMultiplierApplied = false;
        }
        
        public void Dispose()
        {
            RemoveMultiplier();
            _phaseChangedDisposable?.Dispose();
        }
    }
}