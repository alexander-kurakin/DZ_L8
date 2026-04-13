using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage
{
    public class AreaDamageService
    {
        private readonly CollidersRegistryService _collidersRegistry;
        private Buffer<Collider> _collidersBuffer = new Buffer<Collider>(64);
        private Buffer<Entity> _entitiesBuffer = new Buffer<Entity>(64);

        public AreaDamageService(CollidersRegistryService collidersRegistry)
        {
            _collidersRegistry = collidersRegistry;
        }

        public void ApplySphereDamage(
            Vector3 hitCenter,
            float hitRadius,
            float hitDamage,
            LayerMask damageMask,
            Entity damageSource)
        {
            _entitiesBuffer.Count = 0;

            int hitCount = Physics.OverlapSphereNonAlloc(
                hitCenter,
                hitRadius,
                _collidersBuffer.Items,
                damageMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = _collidersBuffer.Items[i];
                Entity hitEntity = _collidersRegistry.GetBy(hitCollider);

                if (hitEntity == null)
                    continue;

                if (ContainsEntity(_entitiesBuffer, hitEntity))
                    continue;

                _entitiesBuffer.Items[_entitiesBuffer.Count] = hitEntity;
                _entitiesBuffer.Count++;
            }

            for (int i = 0; i < _entitiesBuffer.Count; i++)
            {
                Entity damageTarget = _entitiesBuffer.Items[i];
                EntitiesHelper.TryTakeDamageFrom(damageSource, damageTarget, hitDamage);
            }
        }

        private bool ContainsEntity(Buffer<Entity> entitiesBuffer, Entity matchedEntity)
        {
            for (int i = 0; i < entitiesBuffer.Count; i++)
            {
                if (entitiesBuffer.Items[i] == matchedEntity)
                    return true;
            }
            
            return false;
        }
    }
}