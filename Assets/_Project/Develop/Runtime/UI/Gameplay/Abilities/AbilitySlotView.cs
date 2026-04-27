using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class AbilitySlotView : MonoBehaviour, IView
    {
        [SerializeField] private IconView _iconView;
        [SerializeField] private IconTextView _iconTextView;
        
        public IconView IconView => _iconView;
        public IconTextView IconTextView => _iconTextView;
    }
}