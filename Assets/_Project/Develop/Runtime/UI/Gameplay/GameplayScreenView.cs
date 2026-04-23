using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenView : MonoBehaviour, IView
    {
        [field: SerializeField] public IconTextListView WalletIconTextListView { get; private set; }
        [field: SerializeField] public IconTextListView StatsIconTextListView { get; private set; }
    }
}