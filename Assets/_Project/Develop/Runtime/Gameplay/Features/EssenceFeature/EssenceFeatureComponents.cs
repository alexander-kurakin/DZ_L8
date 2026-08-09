using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EnemyWavePreviewType : IEntityComponent
    {
        public WaveEnemyPreviewType Value;
    }

    public class EssenceAmount : IEntityComponent
    {
        public ReactiveVariable<int> Value;
    }

    public class EssenceHoverUnlockRemainingTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class EssenceCanAcceptHover : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class EssenceIsVacuuming : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class EssenceIsCollected : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class EssenceHoverCollider : IEntityComponent
    {
        public Collider Value;
    }

    public class EssenceStartVacuumRequest : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class EssenceHoverReadyEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class EssenceVacuumStartedEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class EssenceCollectedEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }
}
