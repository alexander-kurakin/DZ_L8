using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.AbilitySystems
{
    public class ExplodeAtPointSystem : IInitializableSystem, IDisposableSystem
    {
        private Entity _entity;
        private IDisposable _requestDisposable;
        private ReactiveEvent<Vector3> _dealAreaImpactDamageRequest;

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _dealAreaImpactDamageRequest = entity.DealAreaImpactDamageRequest;
            _requestDisposable = _entity.AbilityUseRequest.Subscribe(OnAbilityUse);
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            _dealAreaImpactDamageRequest?.Invoke(usePoint);
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
        }
    }
}