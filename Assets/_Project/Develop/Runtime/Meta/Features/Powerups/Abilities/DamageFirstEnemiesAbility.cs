using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace _Project.Develop.Runtime.Meta.Features.Powerups.Abilities
{
    public class DamageFirstEnemiesAbility : Powerup, IDisposable
    {
        private readonly Entity _mainHero;
        private readonly PermanentDamageFirstEnemiesConfig _damageFirstEnemiesConfig;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly StageProviderService _stageProviderService;

        private readonly Dictionary<Entity, IDisposable> _spawnedEnemies = new();
        
        private IDisposable _gameplayStateDisposable;
        private int _enemiesDamagedThisWave;
        private int _damageableEnemiesCountThisWave;
        
        public DamageFirstEnemiesAbility(
            Entity mainHero,
            PermanentDamageFirstEnemiesConfig damageFirstEnemiesConfig,
            int currentLevel,
            EntitiesLifeContext entitiesLifeContext,
            StageProviderService stageProviderService) : base(damageFirstEnemiesConfig.ID, currentLevel, damageFirstEnemiesConfig.MaxLevel)
        {
            _mainHero = mainHero;
            _damageFirstEnemiesConfig = damageFirstEnemiesConfig;
            _entitiesLifeContext = entitiesLifeContext;
        }

        public override void Activate()
        {
            _entitiesLifeContext.Added += OnEntityAdded;
            _gameplayStateDisposable = _mainHero.GameplayPhase.Subscribe(OnGameplayPhaseChanged);
        }

        private void OnGameplayPhaseChanged(GameplayStates oldState, GameplayStates newState)
        {
            if (newState == GameplayStates.Preparation)
            {
                _enemiesDamagedThisWave = 0;
                _damageableEnemiesCountThisWave = 0;
                return;
            }
            
            if (newState == GameplayStates.StageProcess)
            {
                _enemiesDamagedThisWave = 0;
                _damageableEnemiesCountThisWave = CalculateDamageableEnemiesCountForCurrentWave();
            }     
        }

        private int CalculateDamageableEnemiesCountForCurrentWave()
        {
            int enemiesCount = _stageProviderService.CurrentStageEnemiesCount;
            
            float percent = _damageFirstEnemiesConfig.PercentOfWaveEnemiesDamaged;
            
            int result = (int)MathF.Ceiling(enemiesCount * percent);
            
            return Math.Min(enemiesCount, Math.Max(0, result));
        }

        private void OnEntityAdded(Entity entity)
        {
            if (_enemiesDamagedThisWave >= _damageableEnemiesCountThisWave)
                return;

            if (entity.TryGetTeam(out ReactiveVariable<Teams> team) == false || team.Value != Teams.Enemies)
                return;

            if (entity.TryGetInSpawnProcess(out ReactiveVariable<bool> inSpawnProcess) == false)
                return;

            if (inSpawnProcess.Value == false)
            {
                ApplyDamage(entity);
                return;
            }
            
            IDisposable entityWaitDisposable = inSpawnProcess.Subscribe((oldValue, newValue) =>
                OnSpawnProcessChanged(entity, oldValue, newValue));
            
            _spawnedEnemies[entity] = entityWaitDisposable;
        }

        private void OnSpawnProcessChanged(Entity entity, bool oldValue, bool newValue)
        {
            if (newValue)
                return;
            
            if (_spawnedEnemies.ContainsKey(entity))
            {
                _spawnedEnemies[entity].Dispose();
                _spawnedEnemies.Remove(entity);
            }
            
            if (_enemiesDamagedThisWave >= _damageableEnemiesCountThisWave)
                return;
                
            ApplyDamage(entity);
        }
        
        private void ApplyDamage(Entity entity)
        {
            if (entity.TryGetMaxHealth(out ReactiveVariable<float> maxHealth) == false)
                return;
            
            float damage = maxHealth.Value * _damageFirstEnemiesConfig.PercentOfEnemyLifeLost;
            
            if (EntitiesHelper.TryTakeDamageFrom(_mainHero, entity, damage))
                _enemiesDamagedThisWave++;
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnEntityAdded;
            _gameplayStateDisposable?.Dispose();
            
            foreach (KeyValuePair<Entity, IDisposable> pair in _spawnedEnemies)
                pair.Value.Dispose();
            
            _spawnedEnemies.Clear();
        }
    }
}