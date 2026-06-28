using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    [RequireComponent(typeof(Collider))]
    public class SectorVolumeRegistrator : MonoBehaviour
    {
        [SerializeField] private SectorBelt _belt;
        [SerializeField] private int _index;
        [SerializeField] private Collider _triggerCollider;

        public SectorId SectorId => new SectorId(_belt, _index);

        public Collider TriggerCollider => _triggerCollider;

        private SectorRegistryService _registeredRegistry;

        private void Reset()
        {
            _triggerCollider = GetComponent<Collider>();

            if (_triggerCollider != null)
                _triggerCollider.isTrigger = true;
        }

        private void OnValidate()
        {
            if (_index < 0)
                _index = 0;

            if (_index >= SectorId.SectorsPerRing)
                _index = SectorId.SectorsPerRing - 1;
        }

        public void Register(SectorRegistryService registry)
        {
            if (registry == null)
                return;

            if (_triggerCollider == null)
                _triggerCollider = GetComponent<Collider>();

            registry.RegisterVolume(SectorId, transform.position, _triggerCollider);
            _registeredRegistry = registry;
        }

        public void Unregister()
        {
            if (_registeredRegistry == null)
                return;

            _registeredRegistry.UnregisterVolume(_triggerCollider);
            _registeredRegistry = null;
        }

        private void OnDestroy()
        {
            Unregister();
        }
    }
}
