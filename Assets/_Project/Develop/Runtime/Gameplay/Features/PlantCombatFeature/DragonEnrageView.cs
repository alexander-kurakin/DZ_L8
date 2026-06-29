using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class DragonEnrageView : EntityView
    {
        [SerializeField] private GameObject _enrageEffectPrefab;
        [SerializeField] private Transform _effectAnchor;
        [SerializeField] private float _enrageEffectScale = 1.2f;
        [SerializeField] private Vector3 _localPositionOffset = new Vector3(0f, -0.75f, 0f);
        [SerializeField] private Vector3 _localScaleMultiplier = new Vector3(2.6f, 0.85f, 2.6f);

        private GameObject _activeEffectInstance;
        private IDisposable _isDeadDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isDeadDisposable = entity.IsDead.Subscribe(OnIsDeadChanged);
        }

        public void PlayEnrage(int stackCount, float enrageScalePerStack)
        {
            if (_enrageEffectPrefab == null)
                return;

            Transform anchor = _effectAnchor != null ? _effectAnchor : transform;
            float effectScale = _enrageEffectScale + stackCount * enrageScalePerStack;

            if (_activeEffectInstance == null)
            {
                _activeEffectInstance = GameplayVfxUtility.SpawnAt(
                    _enrageEffectPrefab,
                    anchor.position,
                    Quaternion.identity,
                    anchor,
                    1f);

                GameplayVfxUtility.PlayLoopingParticleSystemsInChildren(_activeEffectInstance);
                ApplyEnrageTransform(_activeEffectInstance.transform, effectScale);
                return;
            }

            ApplyEnrageTransform(_activeEffectInstance.transform, effectScale);
            GameplayVfxUtility.PlayLoopingParticleSystemsInChildren(_activeEffectInstance);
        }

        private void OnIsDeadChanged(bool oldIsDead, bool isDead)
        {
            if (isDead == false)
                return;

            ClearActiveEffect();
        }

        private void ClearActiveEffect()
        {
            if (_activeEffectInstance == null)
                return;

            Object.Destroy(_activeEffectInstance);
            _activeEffectInstance = null;
        }

        private void ApplyEnrageTransform(Transform effectTransform, float effectScale)
        {
            effectTransform.localPosition = _localPositionOffset;
            effectTransform.localScale = new Vector3(
                effectScale * _localScaleMultiplier.x,
                effectScale * _localScaleMultiplier.y,
                effectScale * _localScaleMultiplier.z);
        }

        public override void Cleanup(Entity entity)
        {
            ClearActiveEffect();
            _isDeadDisposable?.Dispose();
            base.Cleanup(entity);
        }
    }
}
