using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage
{
    public class AreaDamageContactDetectingSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly CollidersRegistryService _collidersRegistryService;
        private Entity _damageSourceEntity;

        private ReactiveVariable<float> _areaImpactRadius;
        
        private LayerMask _areaImpactMask;
        
        private Buffer<Collider> _areaImpactColliders;
        
        private ReactiveEvent<Vector3> _dealAreaImpactDamageRequest;
        private IDisposable _areaImpactDamageRequestDispose;
        
        public AreaDamageContactDetectingSystem(
            CollidersRegistryService collidersRegistryService)
        {
            _collidersRegistryService = collidersRegistryService;
        }

        public void OnInit(Entity entity)
        {
            _damageSourceEntity = entity;
            _areaImpactRadius = entity.AreaImpactRadius;
            _areaImpactMask = entity.AreaImpactMask;
            _areaImpactColliders = entity.AreaImpactCollidersBuffer;
            
            _dealAreaImpactDamageRequest = entity.DealAreaImpactDamageRequest;
            _areaImpactDamageRequestDispose = _dealAreaImpactDamageRequest.Subscribe(OnDealAreaImpactDamageRequest);
        }

        private void OnDealAreaImpactDamageRequest(Vector3 impactPoint)
        {
            _areaImpactColliders.Count = 0;
            
            int overlapHits = Physics.OverlapSphereNonAlloc(
                impactPoint,
                _areaImpactRadius.Value,
                _areaImpactColliders.Items,
                _areaImpactMask
            );
            
            _areaImpactColliders.Count = overlapHits;
            RemoveSelfFromContacts();
        }
        
        private void RemoveSelfFromContacts()
        {
            int nextKeepSlot = 0;

            for (int scanIndex = 0; scanIndex < _areaImpactColliders.Count; scanIndex++)
            {
                Collider overlapCollider = _areaImpactColliders.Items[scanIndex];
                Entity hitEntity = _collidersRegistryService.GetBy(overlapCollider);

                if (hitEntity != null && hitEntity == _damageSourceEntity)
                    continue;

                _areaImpactColliders.Items[nextKeepSlot] = overlapCollider;
                nextKeepSlot++;
            }
            _areaImpactColliders.Count = nextKeepSlot;
            
        }

        public void OnDispose()
        {
            _areaImpactDamageRequestDispose.Dispose();
        }
    }
}