using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Stages
{
    [CreateAssetMenu(
        menuName = "Configs/Gameplay/Stages/NewWaveEnemyPreviewIconsConfig",
        fileName = "WaveEnemyPreviewIconsConfig")]
    public class WaveEnemyPreviewIconsConfig : ScriptableObject
    {
        [SerializeField] private Sprite _catIcon;
        [SerializeField] private Sprite _tankIcon;
        [SerializeField] private Sprite _dragonIcon;

        public Sprite GetSpriteFor(WaveEnemyPreviewType previewType)
        {
            switch (previewType)
            {
                case WaveEnemyPreviewType.Cat:
                    return GetRequiredSprite(_catIcon, previewType);

                case WaveEnemyPreviewType.Tank:
                    return GetRequiredSprite(_tankIcon, previewType);

                case WaveEnemyPreviewType.Dragon:
                    return GetRequiredSprite(_dragonIcon, previewType);

                default:
                    throw new ArgumentOutOfRangeException(nameof(previewType), previewType, null);
            }
        }

        private static Sprite GetRequiredSprite(Sprite sprite, WaveEnemyPreviewType previewType)
        {
            if (sprite == null)
                throw new InvalidOperationException($"Wave preview icon for {previewType} is not assigned in WaveEnemyPreviewIconsConfig.");

            return sprite;
        }
    }
}
