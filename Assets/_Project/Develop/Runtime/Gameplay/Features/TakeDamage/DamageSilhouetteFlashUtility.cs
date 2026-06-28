using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public static class DamageSilhouetteFlashUtility
    {
        private const float PEAK_DURATION_SECONDS = 0.07f;
        private const float RECOVER_DURATION_SECONDS = 0.2f;
        private const float HEAVY_DAMAGE_THRESHOLD = 80f;

        private static readonly int DamageFlashAmountId = Shader.PropertyToID("_DamageFlashAmount");
        private static readonly int DamageFlashColorId = Shader.PropertyToID("_DamageFlashColor");

        private static readonly Color LightFlashColor = new Color(1.8f, 1.8f, 1.8f, 1f);
        private static readonly Color HeavyFlashColor = new Color(2.2f, 0.45f, 0.3f, 1f);

        private static readonly Dictionary<Transform, FlashPlayback> ActiveFlashes = new Dictionary<Transform, FlashPlayback>();

        public static void PlayOnTransform(Transform root, float damage)
        {
            if (root == null)
                return;

            List<MaterialFlashSlot> slots = CollectFlashSlots(root);

            if (slots.Count == 0)
                return;

            if (ActiveFlashes.TryGetValue(root, out FlashPlayback activePlayback))
            {
                activePlayback.Tween?.Kill();
                ResetFlashSlots(activePlayback.Slots);
                ActiveFlashes.Remove(root);
            }

            Color flashColor = damage >= HEAVY_DAMAGE_THRESHOLD ? HeavyFlashColor : LightFlashColor;

            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                MaterialFlashSlot slot = slots[slotIndex];
                slot.Material.SetColor(DamageFlashColorId, flashColor);
                slot.Material.SetFloat(DamageFlashAmountId, 0f);
            }

            FlashPlayback playback = new FlashPlayback
            {
                Slots = slots
            };

            float flashAmount = 0f;
            Sequence flashSequence = DOTween.Sequence();
            flashSequence.Append(
                DOTween.To(
                    () => flashAmount,
                    value =>
                    {
                        flashAmount = value;
                        ApplyFlashAmount(slots, flashAmount);
                    },
                    1f,
                    PEAK_DURATION_SECONDS));
            flashSequence.Append(
                DOTween.To(
                    () => flashAmount,
                    value =>
                    {
                        flashAmount = value;
                        ApplyFlashAmount(slots, flashAmount);
                    },
                    0f,
                    RECOVER_DURATION_SECONDS).SetEase(Ease.OutQuad));
            flashSequence.OnKill(() => FinishFlash(root, playback));
            flashSequence.OnComplete(() => FinishFlash(root, playback));
            flashSequence.SetUpdate(true);

            playback.Tween = flashSequence.Play();
            ActiveFlashes[root] = playback;
        }

        private static List<MaterialFlashSlot> CollectFlashSlots(Transform root)
        {
            List<MaterialFlashSlot> slots = new List<MaterialFlashSlot>();
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];

                if (renderer == null || IsIgnoredRenderer(renderer))
                    continue;

                Material[] materials = renderer.materials;

                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];

                    if (material == null || material.HasProperty(DamageFlashAmountId) == false)
                        continue;

                    slots.Add(new MaterialFlashSlot
                    {
                        Material = material
                    });
                }
            }

            return slots;
        }

        private static void ApplyFlashAmount(List<MaterialFlashSlot> slots, float flashAmount)
        {
            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
                slots[slotIndex].Material.SetFloat(DamageFlashAmountId, flashAmount);
        }

        private static void ResetFlashSlots(List<MaterialFlashSlot> slots)
        {
            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
                slots[slotIndex].Material.SetFloat(DamageFlashAmountId, 0f);
        }

        private static void FinishFlash(Transform root, FlashPlayback playback)
        {
            if (ActiveFlashes.TryGetValue(root, out FlashPlayback activePlayback) == false
                || activePlayback != playback)
            {
                return;
            }

            ResetFlashSlots(playback.Slots);
            ActiveFlashes.Remove(root);
        }

        private static bool IsIgnoredRenderer(Renderer renderer)
        {
            return renderer is ParticleSystemRenderer
                   || renderer is TrailRenderer
                   || renderer is LineRenderer;
        }

        private sealed class FlashPlayback
        {
            public List<MaterialFlashSlot> Slots;
            public Tween Tween;
        }

        private sealed class MaterialFlashSlot
        {
            public Material Material;
        }
    }
}
