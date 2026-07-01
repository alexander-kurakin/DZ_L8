using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerIntegrityFeature
{
    public class TowerIntegrityLeakResolver
    {
        private readonly TowerIntegrityConfig _config;

        public TowerIntegrityLeakResolver(TowerIntegrityConfig config)
        {
            _config = config;
        }

        public int ResolveHits(Entity source)
        {
            if (source == null)
                return _config.DefaultLeakHits;

            if (source.TryGetBodyContactDamage(out _))
                return _config.TankShotHits;

            if (source.TryGetExplosionDamage(out _))
                return _config.CatExplosionHits;

            if (source.TryGetDragonEnrageStackCount(out _))
                return _config.DragonBeamTickHits;

            if (source.TryGetDamagePerTick(out _))
                return _config.DragonBeamTickHits;

            return _config.DefaultLeakHits;
        }
    }
}
