using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class AbilitySlotView : MonoBehaviour, IView
    {
        [SerializeField] private IconView _iconView;
        [SerializeField] private IconTextView _iconTextView;
        [SerializeField] private TextView _countTextView;

        public IconView IconView => _iconView;
        public IconTextView IconTextView => _iconTextView;

        public void SetCostVisible(bool visible)
        {
            _iconTextView.gameObject.SetActive(visible);
        }

        public void SetCountVisible(bool visible)
        {
            if (_countTextView == null)
                return;

            _countTextView.gameObject.SetActive(visible);
        }

        public void SetCountText(string text)
        {
            if (_countTextView == null)
                return;

            _countTextView.SetText(text);
        }
    }
}
