using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore
{
    [CreateAssetMenu(
        menuName = "Configs/Gameplay/Spellcore/NewTowerIntegrityConfig",
        fileName = "TowerIntegrityConfig")]
    public class TowerIntegrityConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _maxHits = 200;

        [SerializeField, Min(1)] private int _catExplosionHits = 1;

        [SerializeField, Min(1)] private int _tankShotHits = 2;

        [SerializeField, Min(1)] private int _dragonBeamTickHits = 1;

        [SerializeField, Min(1)] private int _defaultLeakHits = 1;

        public int MaxHits => _maxHits;

        public int CatExplosionHits => _catExplosionHits;

        public int TankShotHits => _tankShotHits;

        public int DragonBeamTickHits => _dragonBeamTickHits;

        public int DefaultLeakHits => _defaultLeakHits;
    }
}
