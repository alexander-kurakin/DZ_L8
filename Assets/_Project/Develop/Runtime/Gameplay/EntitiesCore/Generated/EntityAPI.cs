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
