using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage
{
    public class DealDamageOnContactSystem : IInitializableSystem, IUpdatableSystem
    {
        private Entity _entity;
        private Buffer<Entity> _contacts;
        private ReactiveVariable<float> _damage;

        private List<Entity> _processedEntities;

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _contacts = entity.ContactEntitiesBuffer;
            _damage = entity.BodyContactDamage;

            _processedEntities = new List<Entity>(_contacts.Items.Length);
        }

        public void OnUpdate(float deltaTime)
        {
            for (int contactIndex = 0; contactIndex < _contacts.Count; contactIndex++)
            {
                Entity contactEntity = _contacts.Items[contactIndex];

                if (ProjectileContactRules.IsValidDamageTarget(contactEntity) == false)
                    continue;

                if (_processedEntities.Contains(contactEntity))
                    continue;

                _processedEntities.Add(contactEntity);

                EntitiesHelper.TryTakeDamageFrom(
                    _entity,
                    contactEntity,
                    _damage.Value);
            }

            for (int processedIndex = _processedEntities.Count - 1; processedIndex >= 0; processedIndex--)
            {
                if (ContainInContacts(_processedEntities[processedIndex]) == false)
                    _processedEntities.RemoveAt(processedIndex);
            }
        }

        public bool ContainInContacts(Entity entity)
        {
            for (int contactIndex = 0; contactIndex < _contacts.Count; contactIndex++)
            {
                if (_contacts.Items[contactIndex] == entity)
                    return true;
            }

            return false;
        }
    }
}
