using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature
{
    public class TowerTakeDamageScreenShakeSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly GameplayJuiceService _gameplayJuiceService;

        private ReactiveEvent<TakeDamageInfo> _damageEvent;
        private IDisposable _damageEventSubscription;

        public TowerTakeDamageScreenShakeSystem(GameplayJuiceService gameplayJuiceService)
        {
            _gameplayJuiceService = gameplayJuiceService;
        }

        public void OnInit(Entity entity)
        {
            _damageEvent = entity.TakeDamageEvent;
            _damageEventSubscription = _damageEvent.Subscribe(OnDamageTaken);
        }

        public void OnDispose()
        {
            _damageEventSubscription?.Dispose();
        }

        private void OnDamageTaken(TakeDamageInfo damageInfo)
        {
            if (damageInfo.Damage <= 0f)
                return;

            _gameplayJuiceService.PlayScreenShakeMedium();
        }
    }
}
