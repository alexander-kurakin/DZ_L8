using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;

namespace Assets._Project.Develop.Runtime.Meta.Features.Powerups
{
    public class PowerupOnAddActivatorSystem : IInitializableSystem, IDisposableSystem
    {
        private PowerupList _powerupList;

        public void OnInit(Entity entity)
        {
            _powerupList = entity.Powerup;

            _powerupList.Added += OnPowerupAdded;

            foreach (Powerup powerup in _powerupList.Elements)
                powerup.Activate();
        }

        private void OnPowerupAdded(Powerup powerup)
        {
            powerup.Activate();
        }

        public void OnDispose()
        {
            _powerupList.Added -= OnPowerupAdded;
        }
    }
}