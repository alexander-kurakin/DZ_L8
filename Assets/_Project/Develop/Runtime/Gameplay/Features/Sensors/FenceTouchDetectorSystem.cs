using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Sensors
{
    public class FenceTouchDetectorSystem : IInitializableSystem, IUpdatableSystem
    {
        private Buffer<Collider> _contacts;
        private ReactiveVariable<bool> _isTouchingFence;
        private LayerMask _fenceMask;
        
        public void OnInit(Entity entity)
        {
            _contacts = entity.ContactCollidersBuffer;
            _fenceMask = entity.FenceMask;
            _isTouchingFence = entity.IsTouchingFence;
        }

        public void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < _contacts.Count; i++)
            {
                Collider contact = _contacts.Items[i];
                
                if (MatchWithFenceLayer(_contacts.Items[i]))
                {
                    _isTouchingFence.Value = true;
                    return;
                }
            }

            _isTouchingFence.Value = false;
        }
        
        private bool MatchWithFenceLayer(Collider collider)
        {
            return ((1 << collider.gameObject.layer) & _fenceMask.value) != 0;
        }
    }
}