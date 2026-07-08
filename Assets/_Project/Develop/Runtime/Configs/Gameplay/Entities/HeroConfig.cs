using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewHeroConfig", fileName = "HeroConfig")]
    public class HeroConfig : EntityConfig
    {
        [field: SerializeField] public Vector3 StartPosition { get; private set; } = Vector3.zero;
    }
}
