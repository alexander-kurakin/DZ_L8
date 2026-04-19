using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage
{
    public class AreaDamageEntitiesFilterSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly CollidersRegistryService _collidersRegistryService;
        
        private Buffer<Collider> _areaDamageImpactColliders;
        private Buffer<Entity> _areaDamageImpactEntities;
        
        public AreaDamageEntitiesFilterSystem(CollidersRegistryService collidersRegistryService)
        {
            _collidersRegistryService = collidersRegistryService;
        }
        
        public void OnInit(Entity entity)
        {
            _areaDamageImpactColliders = entity.AreaImpactCollidersBuffer;
            _areaDamageImpactEntities = entity.AreaImpactEntitiesBuffer;
        }
        
        public void OnUpdate(float deltaTime)
        {
            _areaDamageImpactEntities.Count = 0;
            
            for (int i = 0; i < _areaDamageImpactColliders.Count; i++)
            {
                Collider collider = _areaDamageImpactColliders.Items[i];
                Entity targetEntity = _collidersRegistryService.GetBy(collider);
                
                if (targetEntity != null)
                {
                    _areaDamageImpactEntities.Items[_areaDamageImpactEntities.Count] = targetEntity;
                    _areaDamageImpactEntities.Count++;
                }
            }
            
            _areaDamageImpactColliders.Count = 0;
        }
    }
}