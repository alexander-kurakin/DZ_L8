using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using DamageNumbersPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature
{
    public class PlantSellJuiceService
    {
        private const float REFUND_NUMBER_SCALE = 3f;

        private readonly EssenceConfig _essenceConfig;

        public PlantSellJuiceService(EssenceConfig essenceConfig)
        {
            _essenceConfig = essenceConfig;
        }

        public void SpawnRefundNumber(Vector3 worldPosition, int refund)
        {
            DamageNumber refundNumberPrefab = _essenceConfig.PlantSellRefundNumberPrefab;

            if (refundNumberPrefab == null)
                return;

            refundNumberPrefab.SetScale(REFUND_NUMBER_SCALE);
            refundNumberPrefab.SetColor(Color.green);
            refundNumberPrefab.Spawn(worldPosition, $"+{refund}");
        }
    }
}
