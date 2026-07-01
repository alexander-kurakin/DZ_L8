using System;
using _Project.Develop.Runtime.Gameplay.Features.PlantableObjects;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.AbilitySystems
{
    public class PlantToxicAreaSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly RunEssenceService _runEssenceService;
        private readonly PlantableObjectsFactory _plantableObjectsFactory;
        private readonly PurchasableEntityConfig _purchasableEntityConfig;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;
        private readonly PlantPlacementService _plantPlacementService;
        
        private Entity _parent;
        private IDisposable _requestDisposable;

        public PlantToxicAreaSystem(
            RunEssenceService runEssenceService,
            PlantableObjectsFactory plantableObjectsFactory,
            PurchasableEntityConfig purchasableEntityConfig,
            SpellcoreProgressionService spellcoreProgressionService,
            PlantPlacementService plantPlacementService)
        {
            _runEssenceService = runEssenceService;
            _plantableObjectsFactory = plantableObjectsFactory;
            _purchasableEntityConfig = purchasableEntityConfig;
            _spellcoreProgressionService = spellcoreProgressionService;
            _plantPlacementService = plantPlacementService;
        }

        public void OnInit(Entity entity)
        {
            _parent = entity;
            _requestDisposable = _parent.AbilityUseRequest.Subscribe(OnAbilityUse);
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            if (_spellcoreProgressionService.IsAbilityUnlocked(AbilityType.PlantToxicArea) == false)
                return;

            if (_plantPlacementService.TryResolvePlacement(
                    usePoint,
                    AbilityType.PlantToxicArea,
                    out Vector3 plantPosition,
                    out SectorId sectorId) == false)
                return;

            if (_runEssenceService.Enough(_purchasableEntityConfig.CostInEssence)) 
            {
                int essenceCost = _purchasableEntityConfig.CostInEssence;
                _runEssenceService.Spend(essenceCost);
                Entity plantEntity = _plantableObjectsFactory.Create(
                    plantPosition,
                    _purchasableEntityConfig,
                    sectorId,
                    essenceCost);
                _plantPlacementService.RegisterPlantedEntity(plantEntity, sectorId);
            }            
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
        }
    }
}
