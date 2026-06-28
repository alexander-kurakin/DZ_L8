using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class RunEssenceService
    {
        private readonly ReactiveVariable<int> _amount = new(0);

        public IReadOnlyVariable<int> Amount => _amount;

        public void InitializeForRun(EssenceConfig essenceConfig)
        {
            _amount.Value = essenceConfig.StartEssencePerRun;
        }

        public void ClearForNewRun()
        {
            _amount.Value = 0;
        }

        public bool Enough(int cost)
        {
            return _amount.Value >= cost;
        }

        public void Add(int value)
        {
            if (value <= 0)
                return;

            _amount.Value += value;
        }

        public void Spend(int cost)
        {
            if (Enough(cost) == false)
                throw new System.InvalidOperationException("Not enough Essence.");

            _amount.Value -= cost;
        }

        public int CalculateRefund(int spentCost, EssenceConfig essenceConfig)
        {
            return UnityEngine.Mathf.FloorToInt(spentCost * essenceConfig.PlantSellRefundFraction);
        }
    }
}
