using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewContactTriggerConfig", fileName = "ContactTriggerConfig")]
    public class ContactTriggerConfig : EntityConfig
    {
        [field: SerializeField] public Vector3 ContactTriggerStartPosition { get; private set; } 
            = new Vector3(0,0,-30);
        [field: SerializeField, Min(0)] public float MouseRaycastDistance { get; private set; } = 1000;
    }
}