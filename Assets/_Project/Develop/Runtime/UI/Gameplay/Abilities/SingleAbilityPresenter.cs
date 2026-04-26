using _Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class SingleAbilityPresenter : IPresenter
    {
        private readonly AbilityType _abilityType;
        private readonly AbilityIconsConfig _abilityIconsConfig; 
        private readonly IconView _view;
        
        public SingleAbilityPresenter(
            AbilityType abilityType,
            AbilityIconsConfig abilityIconsConfig,
            IconView view)
        {
            _abilityType = abilityType;
            _abilityIconsConfig = abilityIconsConfig;
            _view = view;
        }
        
        public IconView View => _view;

        public void Initialize()
        {
            _view.SetIcon(_abilityIconsConfig.GetSpriteFor(_abilityType));
        }

        public void Dispose()
        {
        }
    }
}