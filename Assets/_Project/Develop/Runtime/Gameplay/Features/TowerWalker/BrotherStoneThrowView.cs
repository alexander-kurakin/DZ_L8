using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public class BrotherStoneThrowView : EntityView
    {
        private const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0001f;
        private const string THROW_CLIP_NAME = "Take 001";
        private const float TRAIL_TIME_SECONDS = 0.45f;
        private const float TRAIL_MIN_VERTEX_DISTANCE = 0.02f;
        private const float TRAIL_WIDTH_SCALE = 2.8f;
        private const int TRAIL_CAP_VERTICES = 6;
        private const int TRAIL_CORNER_VERTICES = 6;

        private static readonly int IsWalkingKey = Animator.StringToHash("IsWalking");
        private static readonly int PlayIdleFidgetKey = Animator.StringToHash("PlayIdleFidget");
        private static readonly int StoneThrowKey = Animator.StringToHash("StoneThrow");
        private static readonly Color TRAIL_START_COLOR = new Color(0.82f, 0.28f, 1f, 1f);
        private static readonly Color TRAIL_END_COLOR = new Color(0.45f, 0.05f, 0.9f, 0f);

        private static Material _sharedTrailMaterial;
        private static Gradient _sharedTrailGradient;
        private static AnimationCurve _sharedTrailWidthCurve;

        [SerializeField] private Animator _animator;
        [SerializeField] private TowerBrotherStoneThrowConfig _config;
        [SerializeField] private Transform _throwOrigin;

        private Rigidbody _rigidbody;
        private IDisposable _stoneThrowSubscription;
        private ReactiveVariable<bool> _isStoneThrowing;
        private Tween _throwReleaseTween;
        private float _throwAnimationDurationSeconds;

        private void OnValidate()
        {
            _animator ??= GetComponentInChildren<Animator>();
            _throwOrigin ??= transform;
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _rigidbody = entity.Rigidbody;
            _isStoneThrowing = entity.BrotherStoneThrowing;
            _stoneThrowSubscription = entity.BrotherStoneThrowEvent.Subscribe(OnStoneThrown);
            CacheThrowAnimationDuration();
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _throwReleaseTween?.Kill();
            _stoneThrowSubscription?.Dispose();
        }

        private void OnStoneThrown(Entity projectile)
        {
            if (projectile == null || projectile.TryGetTransform(out Transform projectileTransform) == false)
                return;

            Vector3 presentationTarget = projectileTransform.position;

            if (projectile.TryGetMoveDirection(out ReactiveVariable<Vector3> moveDirection))
                presentationTarget = projectileTransform.position + moveDirection.Value;

            BeginThrowPresentation(presentationTarget);
            AttachStoneVisualToProjectile(projectileTransform);
        }

        private void BeginThrowPresentation(Vector3 targetWorldPosition)
        {
            _throwReleaseTween?.Kill();

            RotateTowardsTarget(targetWorldPosition);

            if (_animator != null)
            {
                _animator.SetBool(IsWalkingKey, false);
                _animator.ResetTrigger(PlayIdleFidgetKey);
                _animator.SetTrigger(StoneThrowKey);
            }

            _throwReleaseTween = DOVirtual
                .DelayedCall(_throwAnimationDurationSeconds, ReleaseThrowPresentation)
                .SetUpdate(true)
                .Play();
        }

        private void ReleaseThrowPresentation()
        {
            _isStoneThrowing.Value = false;
        }

        private void CacheThrowAnimationDuration()
        {
            _throwAnimationDurationSeconds = _config != null ? _config.ThrowAnimationDurationSeconds : 0.65f;

            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name != THROW_CLIP_NAME)
                    continue;

                if (clip.length > 0f)
                    _throwAnimationDurationSeconds = clip.length;

                return;
            }
        }

        private void AttachStoneVisualToProjectile(Transform projectileTransform)
        {
            if (_config == null)
                return;

            HideDefaultProjectileVisuals(projectileTransform);

            GameObject stoneInstance = CreatePlaceholderStoneCube(_config.ProjectileScale);
            stoneInstance.transform.SetParent(projectileTransform, false);
            stoneInstance.transform.localPosition = Vector3.zero;
            stoneInstance.transform.localRotation = Quaternion.identity;
            stoneInstance.transform.localScale = Vector3.one * _config.ProjectileScale;

            TrailRenderer trailRenderer = stoneInstance.GetComponent<TrailRenderer>();

            if (trailRenderer != null)
                trailRenderer.Clear();
        }

        private static void HideDefaultProjectileVisuals(Transform projectileRoot)
        {
            Renderer[] renderers = projectileRoot.GetComponentsInChildren<Renderer>(true);

            for (int index = 0; index < renderers.Length; index++)
                renderers[index].enabled = false;

            ParticleSystem[] particleSystems = projectileRoot.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
                particleSystems[index].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static GameObject CreatePlaceholderStoneCube(float projectileScale)
        {
            GameObject stoneInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stoneInstance.name = "BrotherStonePlaceholder";

            Collider collider = stoneInstance.GetComponent<Collider>();

            if (collider != null)
                Destroy(collider);

            ConfigureStoneTrail(stoneInstance, projectileScale);

            return stoneInstance;
        }

        private static void ConfigureStoneTrail(GameObject stoneInstance, float projectileScale)
        {
            TrailRenderer trailRenderer = stoneInstance.AddComponent<TrailRenderer>();
            trailRenderer.time = TRAIL_TIME_SECONDS;
            trailRenderer.minVertexDistance = TRAIL_MIN_VERTEX_DISTANCE;
            trailRenderer.widthMultiplier = projectileScale * TRAIL_WIDTH_SCALE;
            trailRenderer.widthCurve = GetSharedTrailWidthCurve();
            trailRenderer.colorGradient = GetSharedTrailGradient();
            trailRenderer.material = GetSharedTrailMaterial();
            trailRenderer.numCapVertices = TRAIL_CAP_VERTICES;
            trailRenderer.numCornerVertices = TRAIL_CORNER_VERTICES;
            trailRenderer.shadowCastingMode = ShadowCastingMode.Off;
            trailRenderer.receiveShadows = false;
            trailRenderer.textureMode = LineTextureMode.Stretch;
            trailRenderer.alignment = LineAlignment.View;
            trailRenderer.generateLightingData = false;
            trailRenderer.emitting = true;
        }

        private static Material GetSharedTrailMaterial()
        {
            if (_sharedTrailMaterial != null)
                return _sharedTrailMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
                shader = Shader.Find("Particles/Additive");

            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            _sharedTrailMaterial = new Material(shader);
            Color trailColor = TRAIL_START_COLOR;

            if (_sharedTrailMaterial.HasProperty("_BaseColor"))
                _sharedTrailMaterial.SetColor("_BaseColor", trailColor);

            if (_sharedTrailMaterial.HasProperty("_Color"))
                _sharedTrailMaterial.SetColor("_Color", trailColor);

            _sharedTrailMaterial.color = trailColor;
            ConfigureAdditiveTrailMaterial(_sharedTrailMaterial);

            return _sharedTrailMaterial;
        }

        private static void ConfigureAdditiveTrailMaterial(Material material)
        {
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);

            if (material.HasProperty("_Blend"))
                material.SetFloat("_Blend", 1f);

            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.One);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static Gradient GetSharedTrailGradient()
        {
            if (_sharedTrailGradient != null)
                return _sharedTrailGradient;

            _sharedTrailGradient = new Gradient();
            _sharedTrailGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(TRAIL_START_COLOR, 0f),
                    new GradientColorKey(TRAIL_END_COLOR, 1f),
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f),
                });

            return _sharedTrailGradient;
        }

        private static AnimationCurve GetSharedTrailWidthCurve()
        {
            if (_sharedTrailWidthCurve != null)
                return _sharedTrailWidthCurve;

            _sharedTrailWidthCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.35f, 0.85f),
                new Keyframe(1f, 0.1f));

            return _sharedTrailWidthCurve;
        }

        private void RotateTowardsTarget(Vector3 targetWorldPosition)
        {
            if (_rigidbody == null)
                return;

            Vector3 direction = targetWorldPosition - _rigidbody.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= MIN_DIRECTION_SQR_MAGNITUDE)
                return;

            _rigidbody.MoveRotation(Quaternion.LookRotation(direction.normalized, Vector3.up));
        }
    }
}
