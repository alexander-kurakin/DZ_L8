using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using _Project.Develop.Runtime.UI.Gameplay.Essence;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using _Project.Develop.Runtime.UI.Gameplay.LmbFlavorToast;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.CoachHintArrows;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups;
using Assets._Project.Develop.Runtime.UI.Stats;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using _Project.Develop.Runtime.UI.Gameplay.CombatTimeScale;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayTimeScale;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPresentersFactory
    {
        private readonly DIContainer _container;
        private readonly GameplayInputArgs _gameplayInputArgs;
        
        public GameplayPresentersFactory(DIContainer container, GameplayInputArgs gameplayInputArgs)
        {
            _container = container;
            _gameplayInputArgs = gameplayInputArgs;
        }
        
        public WinPopupPresenter CreateWinPopupPresenter(WinPopupView view, WinPopupOpenArgs openArgs)
        {
            return new WinPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<IUISoundService>(),
                _container.Resolve<IBackgroundMusicService>(),
                openArgs);
        }

        public DefeatPopupPresenter CreateDefeatPopupPresenter(DefeatPopupView view)
        {
            return new DefeatPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                _container.Resolve<SceneSwitcherService>(),
                _gameplayInputArgs,
                _container.Resolve<IUISoundService>(),
                _container.Resolve<IBackgroundMusicService>());
        }

        public GameplayScreenPresenter CreateGameplayScreen(GameplayScreenView view)
        {
            return new GameplayScreenPresenter(
                _container.Resolve<ProjectPresentersFactory>(),
                view,
                _container.Resolve<GameplayPresentersFactory>(),
                _container.Resolve<MainHeroHolderService>(),
                _container.Resolve<SpellcoreProgressionService>()
                );
        }

        public GameplayCurrencyHudPresenter CreateGameplayCurrencyHudPresenter(IconTextListView view)
        {
            return new GameplayCurrencyHudPresenter(
                _container.Resolve<WalletService>(),
                _container.Resolve<RunEssenceService>(),
                _container.Resolve<ProjectPresentersFactory>(),
                _container.Resolve<ViewsFactory>(),
                view);
        }

        public AbilityListPresenter CreateAbilityListPresenter(AbilitySlotListView view, Entity mainHero)
        {
            return new AbilityListPresenter(
                _container.Resolve<ProjectPresentersFactory>(),
                this,
                _container.Resolve<ViewsFactory>(),
                view,
                mainHero,
                _container.Resolve<MouseOverUIService>(),
                _container.Resolve<SpellcoreProgressionService>());
        }

        public GameplayStatsPresenter CreateGameplayStatsPresenter(IconTextListView view)
        {
            return new GameplayStatsPresenter(
                view,
                _container.Resolve<ViewsFactory>(),
                _container.Resolve<RunEnemyKillCounterService>(),
                _container.Resolve<ConfigsProviderService>().GetConfig<StatIconsConfig>());
        }

        public StagePresenter CreateStagePresenter(IconTextView view)
        {
            return new StagePresenter(
                view,
                _container.Resolve<StageProviderService>(),
                _container.Resolve<MainHeroHolderService>(),
                _container.Resolve<SpellcoreProgressionService>(),
                _container.Resolve<SurvivalFlowService>());
        }

        public WavePreviewPresenter CreateWavePreviewPresenter(WavePreviewView view)
        {
            return new WavePreviewPresenter(
                view,
                _container.Resolve<StageProviderService>(),
                _container.Resolve<ConfigsProviderService>().GetConfig<WaveEnemyPreviewIconsConfig>(),
                _container.Resolve<SpellcoreProgressionService>(),
                _container.Resolve<MainHeroHolderService>());
        }
        
        public EntityHealthPresenter CreateEntityHealthPresenter(Entity entity, BarWithText view)
        {
            return new EntityHealthPresenter(entity, view);
        }
        
        public EntitiesHealthDisplayPresenter CreateEntitiesHealthDisplayPresenter(EntitiesHealthDisplay view)
        {
            return new EntitiesHealthDisplayPresenter(
                _container.Resolve<EntitiesLifeContext>(),
                view,
                _container.Resolve<ViewsFactory>(),
                this);
        }

        public LmbAbilitySlotPresenter CreateLmbAbilitySlotPresenter(
            AbilitySlotView view,
            Entity mainHero)
        {
            ConfigsProviderService configsProviderService = _container.Resolve<ConfigsProviderService>();

            return new LmbAbilitySlotPresenter(
                configsProviderService.GetConfig<AbilityIconsConfig>(),
                configsProviderService.GetConfig<CurrencyIconsConfig>(),
                _container.Resolve<PlantBuildingBuffService>(),
                view,
                mainHero);
        }

        public PlantBuildingBuffCountdownPresenter CreatePlantBuildingBuffCountdownPresenter(
            ReactiveVariable<float> remainingSeconds,
            TextView view)
        {
            return new PlantBuildingBuffCountdownPresenter(remainingSeconds, view);
        }

        public PlantBuildingBuffTimersDisplayPresenter CreatePlantBuildingBuffTimersDisplayPresenter(
            PlantBuildingBuffTimersDisplay view)
        {
            return new PlantBuildingBuffTimersDisplayPresenter(
                _container.Resolve<PlantBuildingBuffService>(),
                view,
                _container.Resolve<ViewsFactory>(),
                this);
        }

        public CoachHintArrowPresenter CreateCoachHintArrowPresenter(CoachHintArrowView view)
        {
            return new CoachHintArrowPresenter(view);
        }

        public CoachHintArrowsDisplayPresenter CreateCoachHintArrowsDisplayPresenter(CoachHintArrowsDisplay view)
        {
            return new CoachHintArrowsDisplayPresenter(
                _container.Resolve<LmbFlavorToastService>(),
                view,
                _container.Resolve<ViewsFactory>(),
                this,
                _container.Resolve<ConfigsProviderService>(),
                _container.Resolve<SectorRegistryService>(),
                _container.Resolve<PlantPlacementService>(),
                _container.Resolve<WaveSpawnPlanService>(),
                _container.Resolve<SpawnPathPreviewService>(),
                _container.Resolve<PreparationTriggerService>());
        }

        public LmbFlavorToastPresenter CreateLmbFlavorToastPresenter(Transform parent)
        {
            return new LmbFlavorToastPresenter(
                _container.Resolve<LmbFlavorToastService>(),
                parent);
        }

        public CombatTimeScalePresenter CreateCombatTimeScalePresenter(Transform parent)
        {
            return new CombatTimeScalePresenter(
                _container.Resolve<ViewsFactory>(),
                parent,
                _container.Resolve<GameplayTimeScaleService>());
        }
    }
}