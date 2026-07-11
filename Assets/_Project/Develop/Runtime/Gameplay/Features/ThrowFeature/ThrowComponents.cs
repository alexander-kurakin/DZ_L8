using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public struct ThrowReleaseData
    {
        public float Power;
        public Vector3 Direction;
        public Vector3 AimPoint;
        public bool HasAimPoint;
        public Entity Projectile;

        public ThrowReleaseData(float power, Vector3 direction, Vector3 aimPoint, bool hasAimPoint, Entity projectile)
        {
            Power = power;
            Direction = direction;
            AimPoint = aimPoint;
            HasAimPoint = hasAimPoint;
            Projectile = projectile;
        }
    }

    public class IsChargingThrow : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class ThrowChargePower : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class CanChargeThrow : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class CanReleaseThrow : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class ThrowReleased : IEntityComponent
    {
        public ReactiveEvent<ThrowReleaseData> Value;
    }

    public class ThrowTrajectoryPreview : IEntityComponent
    {
        public List<ReactiveVariable<Vector3>> TrajectoryPoints;
        public ReactiveVariable<bool> IsVisible;
    }

    public class IsWatchingThrownProjectile : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class ThrowPostImpactAimLockRemainingTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}
