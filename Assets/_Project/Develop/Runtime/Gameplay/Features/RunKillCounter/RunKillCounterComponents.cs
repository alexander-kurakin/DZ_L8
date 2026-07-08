using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter
{
    public class RunEnemyKillMarker : IEntityComponent
    {
        public ReactiveVariable<bool> IsDead;
    }
}
