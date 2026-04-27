using _Project.Develop.Runtime.Configs.Meta.Powerups;

namespace _Project.Develop.Runtime.Meta.Features.Powerups
{
    public class PermanentPowerupResolver
    {
        private readonly PowerupService _powerupService;
        private readonly PermanentPowerupsConfig _permanentPowerupsConfig;

        public PermanentPowerupResolver(
            PowerupService powerupService,  
            PermanentPowerupsConfig permanentPowerupsConfig)
        {
            _powerupService = powerupService;
            _permanentPowerupsConfig = permanentPowerupsConfig;
        }

        public bool TryGetTowerHealMult(out float towerHealMult)
        {
            if (_powerupService.GetBy(PowerupType.HealExtra).Value == false)
            {
                towerHealMult = 0;
                return false;
            }

            towerHealMult = _permanentPowerupsConfig.TowerExtraHealthPerc;
            return true;
        }

        public bool TryGetFirstEnemiesDamageMult(out float firstEnemiesDamageMult, out int firstEnemiesCount)
        {
            if (_powerupService.GetBy(PowerupType.DamageFirstEnemies).Value == false)
            {
                firstEnemiesDamageMult = 0;
                firstEnemiesCount = 0;
                return false;
            }

            firstEnemiesCount = _permanentPowerupsConfig.FirstEnemiesInWaveCount;
            firstEnemiesDamageMult = _permanentPowerupsConfig.FirstEnemiesInWaveDebuffPerc;
            return true;
        }

        public bool TryGetExplosionDamageMult(out float explosionDamageMult)
        {
            if (_powerupService.GetBy(PowerupType.IncreaseClickDamage).Value == false)
            {
                explosionDamageMult = 0;
                return false;
            }

            explosionDamageMult = _permanentPowerupsConfig.ExplosionDamageIncreasedMult;
            return true;
        }
    }
}