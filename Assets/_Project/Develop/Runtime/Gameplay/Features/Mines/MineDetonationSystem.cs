using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public class MineDetonationSystem : IInitializableSystem, IUpdatableSystem
    {
        private Buffer<Entity> _contacts;
        private List<Entity> _processedEntities;
        private Entity _mineEntity;
        private bool _hasDetonated;
        
        private readonly AreaDamageService _areaDamageService;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        public MineDetonationSystem(AreaDamageService areaDamageService, EntitiesLifeContext entitiesLifeContext)
        {
            _areaDamageService = areaDamageService;
            _entitiesLifeContext = entitiesLifeContext;
        }

        public void OnInit(Entity entity)
        {
            _contacts = entity.ContactEntitiesBuffer;
            _processedEntities = new List<Entity>(_contacts.Items.Length);
            _mineEntity = entity;
        }

        public void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < _contacts.Count; i++)
            {
                if (_hasDetonated)
                    return;
                
                Entity contactEntity = _contacts.Items[i];

                if(_processedEntities.Contains(contactEntity) == false)
                {
                    _processedEntities.Add(contactEntity);

                    _areaDamageService.ApplySphereDamage(
                        _mineEntity.Transform.position, 
                        _mineEntity.MineExplosionRadius.Value, 
                        _mineEntity.MineDamage.Value, 
                        _mineEntity.MineDamageableMask, 
                        _mineEntity);

                    _hasDetonated = true;
                    
                    _entitiesLifeContext.Release(_mineEntity);
                }
            }

            for (int i = _processedEntities.Count - 1; i >= 0; i--)
                if (ContainInContacts(_processedEntities[i]) == false)
                    _processedEntities.RemoveAt(i);
        }

        public bool ContainInContacts(Entity entity)
        {
            for (int i = 0; i < _contacts.Count; i++)
                if (_contacts.Items[i] == entity)
                    return true;

            return false;
        }
    }
}