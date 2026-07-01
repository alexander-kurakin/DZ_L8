using System;
using Assets._Project.Develop.Runtime.Gameplay.States;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups
{
    public class WinPopupOpenArgs
    {
        public RewardsData RewardsData = new RewardsData();
        public WinPopupMode Mode = WinPopupMode.Standard;
        public Action OnContinue;
        public Action OnSecondary;
    }
}
