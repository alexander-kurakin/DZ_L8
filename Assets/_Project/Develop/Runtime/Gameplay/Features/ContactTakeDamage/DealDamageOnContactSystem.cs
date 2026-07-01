using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Utilities;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage
{
    public class DealDamageOnContactSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly GameplayJuiceService _gameplayJuiceService;
        private readonly PlantBuildingBuffJuiceService _plantBuildingBuffJuiceService;

        private Entity _entity;
        private Buffer<Entity> _contacts;
        private ReactiveVariable<float> _damage;
        private TakeDamageVisualKind _visualKind;
        private Entity _damageOwner;

        private List<Entity> _processedEntities;

        public DealDamageOnContactSystem(
            GameplayJuiceService gameplayJuiceService,
            PlantBuildingBuffJuiceService plantBuildingBuffJuiceService)
        {
            _gameplayJuiceService = gameplayJuiceService;
            _plantBuildingBuffJuiceService = plantBuildingBuffJuiceService;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _contacts = entity.ContactEntitiesBuffer;
            _damage = entity.BodyContactDamage;
            _visualKind = entity.HasComponent<ContactDamageVisualKind>()
                ? entity.GetComponent<ContactDamageVisualKind>().Value
                : TakeDamageVisualKind.Default;
            _damageOwner = entity.HasComponent<ContactDamageOwner>()
                ? entity.GetComponent<ContactDamageOwner>().Value
                : null;

            _processedEntities = new List<Entity>(_contacts.Items.Length);
        }

        public void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < _contacts.Count; i++)
            {
                Entity contactEntity = _contacts.Items[i];

                if (ProjectileContactRules.IsValidDamageTarget(contactEntity) == false)
                    continue;

                if (_processedEntities.Contains(contactEntity))
                    continue;

                _processedEntities.Add(contactEntity);

                bool damageApplied = EntitiesHelper.TryTakeDamageFrom(
                    _entity,
                    contactEntity,
                    _damage.Value,
                    _visualKind);

                if (damageApplied && _visualKind == TakeDamageVisualKind.Turret)
                {
                    _gameplayJuiceService.PlayTurretHit(contactEntity);

                    if (_damageOwner != null && _plantBuildingBuffJuiceService.IsBuffed(_damageOwner))
                        _plantBuildingBuffJuiceService.PlayBuffedTurretHit();
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
