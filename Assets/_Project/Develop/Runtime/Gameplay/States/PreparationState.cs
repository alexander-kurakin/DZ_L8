using System;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.Gameplay.Features.LeftClickAbilityPreview;
using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class PreparationState : State, IUpdatableState
    {
        private readonly PreparationTriggerService _preparationTriggerService;
        private readonly ContactTriggerConfig _contactTriggerConfig;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly RaycastConfig _mouseRaycastConfig;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly LmbFrostProjectileService _lmbFrostProjectileService;
        private readonly EssenceFeatureService _essenceFeatureService;
        private readonly PlantBuildingBuffService _plantBuildingBuffService;
        private readonly PlantSellInputService _plantSellInputService;
        private readonly SurvivalFlowService _survivalFlowService;
        private readonly GameplayPopupService _gameplayPopupService;
        private readonly PersistedGoldRewardService _goldRewardService;
        private readonly SceneSwitcherService _sceneSwitcherService;
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly SpellcoreCoachToastService _spellcoreCoachToastService;
        private readonly StageProviderService _stageProviderService;
        private readonly int _levelGoldReward;

        private IMouseInputService _mouseInputService;
        private MouseRaycastService _mouseRaycastService;
        private MouseOverUIService _mouseOverUIService;
        private IBackgroundMusicService _backgroundMusicService;

        private Entity _mainHero;

        public PreparationState(
            PreparationTriggerService preparationTriggerService,
            ConfigsProviderService configsProviderService,
            MainHeroHolderService mainHeroHolderService,
            MouseRaycastService mouseRaycastService,
            IMouseInputService mouseInputService,
            IBackgroundMusicService backgroundMusicService,
            MouseOverUIService mouseOverUIService,
            SpellcoreProgressionService spellcoreProgressionService,
            SectorRegistryService sectorRegistryService,
            LmbFrostProjectileService lmbFrostProjectileService,
            EssenceFeatureService essenceFeatureService,
            PlantBuildingBuffService plantBuildingBuffService,
            PlantSellInputService plantSellInputService,
            SurvivalFlowService survivalFlowService,
            GameplayPopupService gameplayPopupService,
            PersistedGoldRewardService goldRewardService,
            SceneSwitcherService sceneSwitcherService,
            ICoroutinesPerformer coroutinesPerformer,
            SpellcoreCoachToastService spellcoreCoachToastService,
            StageProviderService stageProviderService,
            int levelGoldReward)
        {
            _preparationTriggerService = preparationTriggerService;
            _contactTriggerConfig = configsProviderService.GetConfig<ContactTriggerConfig>();
            _mainHeroHolderService = mainHeroHolderService;
            _mouseRaycastConfig = configsProviderService.GetConfig<RaycastConfig>();
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            _backgroundMusicService = backgroundMusicService;
            _mouseOverUIService = mouseOverUIService;
            _spellcoreProgressionService = spellcoreProgressionService;
            _sectorRegistryService = sectorRegistryService;
            _lmbFrostProjectileService = lmbFrostProjectileService;
            _essenceFeatureService = essenceFeatureService;
            _plantBuildingBuffService = plantBuildingBuffService;
            _plantSellInputService = plantSellInputService;
            _survivalFlowService = survivalFlowService;
            _gameplayPopupService = gameplayPopupService;
            _goldRewardService = goldRewardService;
            _sceneSwitcherService = sceneSwitcherService;
            _coroutinesPerformer = coroutinesPerformer;
            _spellcoreCoachToastService = spellcoreCoachToastService;
            _stageProviderService = stageProviderService;
            _levelGoldReward = levelGoldReward;
        }

        public override void Enter()
        {
            base.Enter();

            _preparationTriggerService.Create(_contactTriggerConfig.ContactTriggerStartPosition);

            _mainHero = _mainHeroHolderService.MainHero;

            _mainHero.GameplayPhase.Value = GameplayStates.Preparation;
            _mainHeroHolderService.TowerWalker.GameplayPhase.Value = _mainHeroHolderService.MainHero.GameplayPhase.Value;
            _mainHeroHolderService.TowerBrother.GameplayPhase.Value = _mainHeroHolderService.MainHero.GameplayPhase.Value;

            _mainHero.AbilityUserActiveAbility.Value = _spellcoreProgressionService.HasAnyPlantAbilityUnlocked()
                ? _mainHero.AbilityUserPlantAbilityPreference.Value
                : AbilityType.LeftClickAtPoint;

            _spellcoreProgressionService.OnPreparationEntered();

            int upcomingWaveNumber = _spellcoreProgressionService.UpcomingWaveNumber;
            _spellcoreCoachToastService.TryShowPreparationHints(
                upcomingWaveNumber,
                _stageProviderService.GetWaveEnemyPreviewTypesForWave(upcomingWaveNumber));

            _essenceFeatureService.TryGrantBailoutOnPreparation(
                _mainHeroHolderService.MainHero,
                _spellcoreProgressionService.CompletedWaves);

            _essenceFeatureService.ActivateAutoHoverForAllPickupsOnPreparation();

            _lmbFrostProjectileService.ClearQueuedProjectileLaunch();
            _plantBuildingBuffService.ClearForNewRun();

            _backgroundMusicService.Play(BackgroundMusicTrackIDs.Preparation);

            TryShowSurvivalPopup();
        }

        public void Update(float deltaTime)
        {
            if (_survivalFlowService.IsSurvivalOfferPending || _survivalFlowService.IsSurvivalMilestonePending)
                return;

            _preparationTriggerService.Update(deltaTime);

            if (_preparationTriggerService.PrepareTriggerClicked.Value)
                return;

            if (_mouseOverUIService.IsPointerOverUI(_mouseInputService.PointerScreenPosition))
                return;

            if (MouseClickedOnPlacementSurface(out Vector3 hitPoint))
            {
                if (_plantSellInputService.TryHandleSellClick(hitPoint))
                    return;

                _mainHero.AbilityUserAllAbilities[_mainHero.AbilityUserActiveAbility.Value]
                    .AbilityUseRequest.Invoke(hitPoint);
            }
        }

        private void TryShowSurvivalPopup()
        {
            if (_survivalFlowService.IsSurvivalOfferPending)
            {
                int goldReward = _levelGoldReward;

                if (_survivalFlowService.TryConsumeCampaignCompletionGoldGrant())
                    _goldRewardService.AddGoldAndPersist(goldReward);

                WinPopupOpenArgs openArgs = new WinPopupOpenArgs
                {
                    Mode = WinPopupMode.SurvivalOffer,
                    RewardsData = new RewardsData { RewardGold = goldReward },
                    OnContinue = OnEnterSurvivalModeClicked,
                    OnSecondary = SwitchToMainMenu
                };

                _gameplayPopupService.OpenWinPopup(openArgs);
                return;
            }

            if (_survivalFlowService.IsSurvivalMilestonePending)
            {
                int goldReward = SurvivalFlowService.SURVIVAL_MILESTONE_BONUS_GOLD;

                if (_survivalFlowService.TryConsumeMilestoneGoldGrant())
                    _goldRewardService.AddGoldAndPersist(goldReward);

                WinPopupOpenArgs openArgs = new WinPopupOpenArgs
                {
                    Mode = WinPopupMode.SurvivalMilestone,
                    RewardsData = new RewardsData { RewardGold = goldReward },
                    OnContinue = OnContinueSurvivalAfterMilestone,
                    OnSecondary = SwitchToMainMenu
                };

                _gameplayPopupService.OpenWinPopup(openArgs);
            }
        }

        private void OnEnterSurvivalModeClicked()
        {
            _survivalFlowService.EnterSurvivalMode();
            _spellcoreProgressionService.OnSurvivalModeEntered();
        }

        private void OnContinueSurvivalAfterMilestone()
        {
            _survivalFlowService.ClearSurvivalMilestonePending();
        }

        private void SwitchToMainMenu()
        {
            _coroutinesPerformer.StartPerform(_sceneSwitcherService.ProcessSwitchTo(Scenes.MainMenu));
        }

        private bool MouseClickedOnPlacementSurface(out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;

            if (_mouseInputService.FireButtonPressed == false)
                return false;

            return SectorSurfaceClickUtility.TryGetArenaPlanePoint(
                _mouseRaycastService,
                _mouseInputService.PointerScreenPosition,
                _sectorRegistryService,
                out hitPoint);
        }

        public override void Exit()
        {
            base.Exit();

            _preparationTriggerService.Cleanup();
        }
    }
}
