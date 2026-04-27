using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public class MineDetonationSystem : IInitializableSystem, IUpdatableSystem
    {
        private Buffer<Entity> _contacts;
        private List<Entity> _processedEntities;
        private Entity _mineEntity;
        private Transform _mineTransform;
        private ReactiveVariable<Teams> _mineTeam;
        private bool _hasDetonated;
        private ReactiveEvent<Vector3> _dealAreaImpactDamageRequest;
        
        private readonly EntitiesLifeContext _entitiesLifeContext;


        public MineDetonationSystem(EntitiesLifeContext entitiesLifeContext)
        {
            _entitiesLifeContext = entitiesLifeContext;
        }

        public void OnInit(Entity entity)
        {
            _contacts = entity.ContactEntitiesBuffer;
            _processedEntities = new List<Entity>(_contacts.Items.Length);
            _mineEntity = entity;
            _dealAreaImpactDamageRequest = entity.DealAreaImpactDamageRequest;
            _mineTransform = entity.Transform;
            _mineTeam = entity.Team;
        }

        public void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < _contacts.Count; i++)
            {
                if (_hasDetonated)
                    return;

                Entity contactEntity = _contacts.Items[i];

                if (_processedEntities.Contains(contactEntity))
                    continue;

                _processedEntities.Add(contactEntity);

                if (IsSameTeam(contactEntity))
                    continue;

                _dealAreaImpactDamageRequest?.Invoke(_mineTransform.position);

                _hasDetonated = true;

                _entitiesLifeContext.Release(_mineEntity);
            }
            
            for (int i = _processedEntities.Count - 1; i >= 0; i--)
                if (ContainInContacts(_processedEntities[i]) == false)
                    _processedEntities.RemoveAt(i);
        }

        private bool IsSameTeam(Entity contactEntity)
        {
            if (contactEntity.TryGetTeam(out ReactiveVariable<Teams> contactTeam))
                return _mineTeam.Value == contactTeam.Value;

            return false;
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