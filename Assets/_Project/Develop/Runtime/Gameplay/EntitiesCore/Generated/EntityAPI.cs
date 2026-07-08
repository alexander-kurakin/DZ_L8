namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
	public partial class Entity
	{
		public Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter.RunEnemyKillMarker RunEnemyKillMarkerC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter.RunEnemyKillMarker>();

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddRunEnemyKillMarker(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean> isDead)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter.RunEnemyKillMarker() {IsDead = isDead}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.EquippedProjectileModifiers EquippedProjectileModifiersC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.EquippedProjectileModifiers>();

		public System.Collections.Generic.List<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType> EquippedProjectileModifiers => EquippedProjectileModifiersC.Value;

		public bool TryGetEquippedProjectileModifiers(out System.Collections.Generic.List<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.EquippedProjectileModifiers component);
			if(result)
				value = component.Value;
			else
				value = default(System.Collections.Generic.List<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddEquippedProjectileModifiers()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.EquippedProjectileModifiers() { Value = new System.Collections.Generic.List<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddEquippedProjectileModifiers(System.Collections.Generic.List<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.EquippedProjectileModifiers() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ActiveProjectileModifier ActiveProjectileModifierC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ActiveProjectileModifier>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType> ActiveProjectileModifier => ActiveProjectileModifierC.Value;

		public bool TryGetActiveProjectileModifier(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ActiveProjectileModifier component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddActiveProjectileModifier()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ActiveProjectileModifier() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddActiveProjectileModifier(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ModifierType> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers.ActiveProjectileModifier() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileSpeed ProjectileSpeedC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileSpeed>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> ProjectileSpeed => ProjectileSpeedC.Value;

		public bool TryGetProjectileSpeed(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileSpeed component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddProjectileSpeed()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileSpeed() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddProjectileSpeed(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileSpeed() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileDamage ProjectileDamageC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileDamage>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> ProjectileDamage => ProjectileDamageC.Value;

		public bool TryGetProjectileDamage(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileDamage component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddProjectileDamage()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileDamage() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddProjectileDamage(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileDamage() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectilePierceCount ProjectilePierceCountC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectilePierceCount>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Int32> ProjectilePierceCount => ProjectilePierceCountC.Value;

		public bool TryGetProjectilePierceCount(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Int32> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectilePierceCount component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Int32>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddProjectilePierceCount()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectilePierceCount() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Int32>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddProjectilePierceCount(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Int32> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectilePierceCount() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileOwner ProjectileOwnerC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileOwner>();

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity ProjectileOwner => ProjectileOwnerC.Value;

		public bool TryGetProjectileOwner(out Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileOwner component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddProjectileOwner()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileOwner() { Value = new Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddProjectileOwner(Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature.ProjectileOwner() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveDirection MoveDirectionC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveDirection>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3> MoveDirection => MoveDirectionC.Value;

		public bool TryGetMoveDirection(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveDirection component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddMoveDirection()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveDirection() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddMoveDirection(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveDirection() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveSpeed MoveSpeedC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveSpeed>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> MoveSpeed => MoveSpeedC.Value;

		public bool TryGetMoveSpeed(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveSpeed component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddMoveSpeed()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveSpeed() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddMoveSpeed(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.MoveSpeed() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.IsMoving IsMovingC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.IsMoving>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean> IsMoving => IsMovingC.Value;

		public bool TryGetIsMoving(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.IsMoving component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddIsMoving()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.IsMoving() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddIsMoving(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.IsMoving() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.CanMove CanMoveC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.CanMove>();

		public Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition CanMove => CanMoveC.Value;

		public bool TryGetCanMove(out Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.CanMove component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddCanMove(Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.CanMove() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationDirection RotationDirectionC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationDirection>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3> RotationDirection => RotationDirectionC.Value;

		public bool TryGetRotationDirection(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationDirection component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddRotationDirection()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationDirection() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddRotationDirection(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<UnityEngine.Vector3> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationDirection() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationSpeed RotationSpeedC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationSpeed>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> RotationSpeed => RotationSpeedC.Value;

		public bool TryGetRotationSpeed(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationSpeed component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddRotationSpeed()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationSpeed() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddRotationSpeed(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Single> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.RotationSpeed() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.CanRotate CanRotateC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.CanRotate>();

		public Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition CanRotate => CanRotateC.Value;

		public bool TryGetCanRotate(out Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.CanRotate component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddCanRotate(Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature.CanRotate() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.MainHero.IsMainHero IsMainHeroC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.MainHero.IsMainHero>();

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddIsMainHero()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.MainHero.IsMainHero() ); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.AI.CurrentTarget CurrentTargetC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.AI.CurrentTarget>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity> CurrentTarget => CurrentTargetC.Value;

		public bool TryGetCurrentTarget(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.AI.CurrentTarget component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddCurrentTarget()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.AI.CurrentTarget() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddCurrentTarget(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.AI.CurrentTarget() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Features.AI.IsCurrentlyIdle IsCurrentlyIdleC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Features.AI.IsCurrentlyIdle>();

		public Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean> IsCurrentlyIdle => IsCurrentlyIdleC.Value;

		public bool TryGetIsCurrentlyIdle(out Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean> value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Features.AI.IsCurrentlyIdle component);
			if(result)
				value = component.Value;
			else
				value = default(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean>);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddIsCurrentlyIdle()
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.AI.IsCurrentlyIdle() { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean>() }); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddIsCurrentlyIdle(Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<System.Boolean> value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Features.AI.IsCurrentlyIdle() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Common.RigidbodyComponent RigidbodyC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Common.RigidbodyComponent>();

		public UnityEngine.Rigidbody Rigidbody => RigidbodyC.Value;

		public bool TryGetRigidbody(out UnityEngine.Rigidbody value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Common.RigidbodyComponent component);
			if(result)
				value = component.Value;
			else
				value = default(UnityEngine.Rigidbody);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddRigidbody(UnityEngine.Rigidbody value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Common.RigidbodyComponent() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Common.TransformComponent TransformC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Common.TransformComponent>();

		public UnityEngine.Transform Transform => TransformC.Value;

		public bool TryGetTransform(out UnityEngine.Transform value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Common.TransformComponent component);
			if(result)
				value = component.Value;
			else
				value = default(UnityEngine.Transform);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddTransform(UnityEngine.Transform value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Common.TransformComponent() {Value = value}); 
		}

		public Assets._Project.Develop.Runtime.Gameplay.Common.AnimatorComponent AnimatorC => GetComponent<Assets._Project.Develop.Runtime.Gameplay.Common.AnimatorComponent>();

		public UnityEngine.Animator Animator => AnimatorC.Value;

		public bool TryGetAnimator(out UnityEngine.Animator value)
		{
			bool result = TryGetComponent(out Assets._Project.Develop.Runtime.Gameplay.Common.AnimatorComponent component);
			if(result)
				value = component.Value;
			else
				value = default(UnityEngine.Animator);
			return result;
		}

		public Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity AddAnimator(UnityEngine.Animator value)
		{
			return AddComponent(new Assets._Project.Develop.Runtime.Gameplay.Common.AnimatorComponent() {Value = value}); 
		}

	}
}
