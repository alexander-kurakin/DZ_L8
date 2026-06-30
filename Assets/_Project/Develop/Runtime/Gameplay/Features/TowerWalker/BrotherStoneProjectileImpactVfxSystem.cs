using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public class BrotherStoneProjectileImpactVfxSystem : IInitializableSystem, IUpdatableSystem
    {
        private const float IMPACT_BURST_START_SCALE = 0.15f;
        private const float IMPACT_BURST_END_SCALE = 1.1f;
        private const float IMPACT_BURST_DURATION_SECONDS = 0.22f;
        private const float IMPACT_BURST_LIFETIME_SECONDS = 0.35f;

        private static readonly Color IMPACT_BURST_COLOR = new Color(0.82f, 0.28f, 1f, 0.75f);

        private static Material _sharedImpactMaterial;

        private Transform _projectileTransform;
        private ReactiveVariable<bool> _isTouchAnotherTeam;
        private bool _impactSpawned;

        public void OnInit(Entity entity)
        {
            entity.TryGetTransform(out _projectileTransform);
            entity.TryGetIsTouchAnotherTeam(out _isTouchAnotherTeam);
        }

        public void OnUpdate(float deltaTime)
        {
            if (_impactSpawned)
                return;

            if (_isTouchAnotherTeam == null || _isTouchAnotherTeam.Value == false)
                return;

            if (_projectileTransform == null)
                return;

            _impactSpawned = true;
            SpawnImpactBurst(_projectileTransform.position);
        }

        private static void SpawnImpactBurst(Vector3 worldPosition)
        {
            GameObject burstInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            burstInstance.name = "BrotherStoneImpactBurst";

            Collider collider = burstInstance.GetComponent<Collider>();

            if (collider != null)
                Object.Destroy(collider);

            burstInstance.transform.position = worldPosition;
            burstInstance.transform.localScale = Vector3.one * IMPACT_BURST_START_SCALE;

            MeshRenderer meshRenderer = burstInstance.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
                meshRenderer.sharedMaterial = GetSharedImpactMaterial();

            burstInstance.transform
                .DOScale(IMPACT_BURST_END_SCALE, IMPACT_BURST_DURATION_SECONDS)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .Play();

            Object.Destroy(burstInstance, IMPACT_BURST_LIFETIME_SECONDS);
        }

        private static Material GetSharedImpactMaterial()
        {
            if (_sharedImpactMaterial != null)
                return _sharedImpactMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
                shader = Shader.Find("Particles/Additive");

            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            _sharedImpactMaterial = new Material(shader);

            if (_sharedImpactMaterial.HasProperty("_BaseColor"))
                _sharedImpactMaterial.SetColor("_BaseColor", IMPACT_BURST_COLOR);

            if (_sharedImpactMaterial.HasProperty("_Color"))
                _sharedImpactMaterial.SetColor("_Color", IMPACT_BURST_COLOR);

            _sharedImpactMaterial.color = IMPACT_BURST_COLOR;
            ConfigureAdditiveMaterial(_sharedImpactMaterial);

            return _sharedImpactMaterial;
        }

        private static void ConfigureAdditiveMaterial(Material material)
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
    }
}
