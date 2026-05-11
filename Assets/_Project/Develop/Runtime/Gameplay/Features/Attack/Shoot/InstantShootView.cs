using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    public class InstantShootView : EntityView
    {
        [SerializeField] private GameSoundsIDs _soundToPlay;
        
        private ReactiveEvent _attackDelayEndEvent;
        private IDisposable _attackDelayEndDisposable;
        
        protected override void OnEntityStartedWork(Entity entity)
        {
            _attackDelayEndEvent = entity.AttackDelayEndEvent;
            _attackDelayEndDisposable = _attackDelayEndEvent.Subscribe(OnAttackDelayEnd);
        }

        private void OnAttackDelayEnd()
        {
            GameSoundsService.PlayGlobal(_soundToPlay);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _attackDelayEndDisposable.Dispose();
        }
    }
}