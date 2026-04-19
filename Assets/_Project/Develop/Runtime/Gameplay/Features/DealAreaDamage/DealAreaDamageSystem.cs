using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage
{
    public class DealAreaDamageSystem : IInitializableSystem, IUpdatableSystem
    {
        private Buffer<Entity> _areaImpactEntities;
        private ReactiveVariable<float> _areaImpactDamage;
        private Entity _damageSourceEntity;
        
        private List<Entity> _processedEntities;
        
        public void OnInit(Entity entity)
        {
            _damageSourceEntity =  entity;
            
            _areaImpactEntities = entity.AreaImpactEntitiesBuffer;
            _areaImpactDamage = entity.AreaImpactDamage;
            
            _processedEntities = new List<Entity>(_areaImpactEntities.Items.Length);
        }
        
        public void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < _areaImpactEntities.Count; i++)
            {
                Entity targetEntity = _areaImpactEntities.Items[i];

                if (_processedEntities.Contains(targetEntity) == false)
                {
                    _processedEntities.Add(targetEntity);

                    EntitiesHelper.TryTakeDamageFrom(_damageSourceEntity, targetEntity, _areaImpactDamage.Value);
                }
            }
        
            _areaImpactEntities.Count = 0;
            _processedEntities.Clear();
        }
    }
}