using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using _Project.Develop.Runtime.UI.Gameplay.GnomeKillToast;
using _Project.Develop.Runtime.UI.Gameplay.ThrowCharge;
using _Project.Develop.Runtime.UI.Gameplay.ThrowCrosshair;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenView : MonoBehaviour, IView
    {
        [field: SerializeField] public IconTextListView WalletIconTextListView { get; private set; }
        [field: SerializeField] public IconTextListView StatsIconTextListView { get; private set; }
        [field: SerializeField] public IconTextView StageNumberView { get; private set; }
        [field: SerializeField] public EntitiesHealthDisplay EntitiesHealthDisplay { get; private set; }
        [field: SerializeField] public EntitiesThrowChargeDisplay EntitiesThrowChargeDisplay { get; private set; }
        [field: SerializeField] public ThrowCrosshairView ThrowCrosshairView { get; private set; }
        [field: SerializeField] public GnomeKillToastView GnomeKillToastView { get; private set; }
    }
}

