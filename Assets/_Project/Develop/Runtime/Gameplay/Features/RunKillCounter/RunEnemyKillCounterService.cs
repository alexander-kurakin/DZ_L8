using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter
{
    public class RunEnemyKillCounterService : IDisposable
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly ReactiveVariable<int> _killCount = new(0);
        private readonly Dictionary<Entity, IDisposable> _enemyDeathSubscriptions = new();

        public RunEnemyKillCounterService(EntitiesLifeContext entitiesLifeContext)
        {
            _entitiesLifeContext = entitiesLifeContext;

            _entitiesLifeContext.Added += OnEntityAdded;
            _entitiesLifeContext.Released += OnEntityReleased;
        }

        public IReadOnlyVariable<int> KillCount => _killCount;

        public void InitializeForRun()
        {
            _killCount.Value = 0;
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnEntityAdded;
            _entitiesLifeContext.Released -= OnEntityReleased;

            foreach (KeyValuePair<Entity, IDisposable> subscription in _enemyDeathSubscriptions)
                subscription.Value.Dispose();

            _enemyDeathSubscriptions.Clear();
        }

        private void OnEntityAdded(Entity entity)
        {
            if (entity.TryGetTeam(out ReactiveVariable<Teams> team) == false)
                return;

            if (team.Value != Teams.Enemies)
                return;

            if (entity.TryGetComponent(out EnemyWavePreviewType previewTypeComponent) == false)
                return;

            IDisposable deathSubscription = entity.IsDead.Subscribe(OnEnemyDeathStateChanged);
            _enemyDeathSubscriptions[entity] = deathSubscription;
        }

        private void OnEnemyDeathStateChanged(bool oldValue, bool isDead)
        {
            if (oldValue == false && isDead)
                _killCount.Value++;
        }

        private void OnEntityReleased(Entity entity)
        {
            if (_enemyDeathSubscriptions.TryGetValue(entity, out IDisposable subscription) == false)
                return;

            subscription.Dispose();
            _enemyDeathSubscriptions.Remove(entity);
        }
    }
}
