using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff
{
    public class PlantBuildingBuffService
    {
        private struct ActiveBuff
        {
            public Entity PlantEntity;
            public float RemainingSeconds;
            public ReactiveVariable<float> RemainingSecondsReactive;
            public GameObject OrbsInstance;
        }

        private readonly PlantPlacementService _plantPlacementService;
        private readonly SectorMembershipService _sectorMembershipService;
        private readonly SpellcoreCombatConfig _spellcoreCombatConfig;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly RunEssenceService _runEssenceService;

        private readonly List<ActiveBuff> _activeBuffs = new();

        private bool _hasStrongerBuffPowerup;
        private float _strongerBuffDamageMultiplier;
        private float _strongerBuffEssenceCostMultiplier = 1f;

        public event Action<Entity> BuffTimerStarted;
        public event Action<Entity> BuffTimerEnded;

        public PlantBuildingBuffService(            PlantPlacementService plantPlacementService,
            SectorMembershipService sectorMembershipService,
            SpellcoreCombatConfig spellcoreCombatConfig,
            EntitiesLifeContext entitiesLifeContext,
            RunEssenceService runEssenceService)
        {
            _plantPlacementService = plantPlacementService;
            _sectorMembershipService = sectorMembershipService;
            _spellcoreCombatConfig = spellcoreCombatConfig;
            _entitiesLifeContext = entitiesLifeContext;
            _runEssenceService = runEssenceService;
            _entitiesLifeContext.Released += OnEntityReleased;
        }

        public void SetStrongerBuffPowerup(float damageMultiplier, float essenceCostMultiplier)
        {
            _hasStrongerBuffPowerup = true;
            _strongerBuffDamageMultiplier = damageMultiplier;
            _strongerBuffEssenceCostMultiplier = essenceCostMultiplier;
        }

        private float ResolveBuffDamageMultiplier()
        {
            if (_hasStrongerBuffPowerup)
                return _strongerBuffDamageMultiplier;

            return _spellcoreCombatConfig.BuildingBuffDamageMultiplier;
        }

        public int GetBuffEssenceCost()
        {
            return Mathf.CeilToInt(_spellcoreCombatConfig.BuildingBuffEssenceCost * _strongerBuffEssenceCostMultiplier);
        }

        public int ActiveBuffCount => _activeBuffs.Count;

        public int MaxActiveBuffCount => _spellcoreCombatConfig.BuildingBuffMaxActiveCount;

        public bool TryApplyBuffAtWorldPosition(Vector3 worldPosition, BuildingBuffCastAbilityConfig lmbConfig)
        {
            SectorId sectorId = _sectorMembershipService.ResolveSectorAtClick(worldPosition);

            if (_plantPlacementService.TryGetPlantAtSector(sectorId, out Entity plantEntity) == false)
                return false;

            int buffEssenceCost = GetBuffEssenceCost();
            float buffDurationSeconds = _spellcoreCombatConfig.BuildingBuffDurationSeconds;

            if (_runEssenceService.Enough(buffEssenceCost) == false)
                return false;

            if (TryRefreshExistingBuff(plantEntity, buffDurationSeconds))
            {
                _runEssenceService.Spend(buffEssenceCost);
                return true;
            }

            if (_activeBuffs.Count >= _spellcoreCombatConfig.BuildingBuffMaxActiveCount)
                RemoveBuffAt(0);

            if (plantEntity.TryGetTransform(out Transform plantTransform) == false)
                return false;

            GameObject orbsInstance = SpawnBuffOrbs(lmbConfig, plantTransform);
            ReactiveVariable<float> remainingSecondsReactive = AttachRemainingSecondsReactive(plantEntity, buffDurationSeconds);

            _activeBuffs.Add(new ActiveBuff
            {
                PlantEntity = plantEntity,
                RemainingSeconds = buffDurationSeconds,
                RemainingSecondsReactive = remainingSecondsReactive,
                OrbsInstance = orbsInstance,
            });

            BuffTimerStarted?.Invoke(plantEntity);

            _runEssenceService.Spend(buffEssenceCost);

            return true;
        }

        public float GetDamageMultiplier(Entity plantSource)
        {
            if (plantSource == null)
                return 1f;

            for (int index = 0; index < _activeBuffs.Count; index++)
            {
                if (_activeBuffs[index].PlantEntity == plantSource)
                    return ResolveBuffDamageMultiplier();
            }

            return 1f;
        }

        public bool IsBuffed(Entity plantSource)
        {
            return GetDamageMultiplier(plantSource) > 1f;
        }

        public bool TryGetRemainingSeconds(Entity plantEntity, out ReactiveVariable<float> remainingSeconds)
        {
            remainingSeconds = null;

            if (plantEntity == null)
                return false;

            if (plantEntity.TryGetComponent(out BuildingBuffRemainingSeconds component) == false)
                return false;

            remainingSeconds = component.Value;
            return remainingSeconds != null;
        }

        public void Tick(float deltaTime)
        {
            for (int index = _activeBuffs.Count - 1; index >= 0; index--)
            {
                ActiveBuff buff = _activeBuffs[index];
                buff.RemainingSeconds -= deltaTime;

                if (buff.RemainingSeconds > 0f)
                {
                    buff.RemainingSecondsReactive.Value = buff.RemainingSeconds;
                    _activeBuffs[index] = buff;
                    continue;
                }

                RemoveBuffAt(index);
            }
        }
        public void ClearForNewRun()
        {
            for (int index = _activeBuffs.Count - 1; index >= 0; index--)
                RemoveBuffAt(index);
        }

        private bool TryRefreshExistingBuff(Entity plantEntity, float buffDurationSeconds)
        {
            for (int index = 0; index < _activeBuffs.Count; index++)
            {
                if (_activeBuffs[index].PlantEntity != plantEntity)
                    continue;

                ActiveBuff buff = _activeBuffs[index];
                buff.RemainingSeconds = buffDurationSeconds;
                buff.RemainingSecondsReactive.Value = buffDurationSeconds;
                _activeBuffs[index] = buff;
                return true;
            }

            return false;
        }

        private ReactiveVariable<float> AttachRemainingSecondsReactive(Entity plantEntity, float initialSeconds)
        {
            if (plantEntity.TryGetComponent(out BuildingBuffRemainingSeconds component) == false)
            {
                ReactiveVariable<float> remainingSecondsReactive = new ReactiveVariable<float>(initialSeconds);
                plantEntity.AddComponent(new BuildingBuffRemainingSeconds { Value = remainingSecondsReactive });
                return remainingSecondsReactive;
            }

            component.Value.Value = initialSeconds;
            return component.Value;
        }

        private void DetachRemainingSecondsReactive(Entity plantEntity)
        {
            if (plantEntity == null)
                return;

            if (plantEntity.RemoveComponent<BuildingBuffRemainingSeconds>() == false)
                return;

            BuffTimerEnded?.Invoke(plantEntity);
        }

        private static GameObject SpawnBuffOrbs(BuildingBuffCastAbilityConfig lmbConfig, Transform plantTransform)
        {
            if (lmbConfig.FrostTargetOrbsPrefab == null)
                return null;

            GameObject orbsInstance = GameplayVfxUtility.SpawnAt(
                lmbConfig.FrostTargetOrbsPrefab,
                plantTransform.position,
                Quaternion.identity,
                plantTransform,
                1f);

            if (orbsInstance == null)
                return null;

            orbsInstance.transform.localScale = Vector3.one * lmbConfig.FrostTargetOrbsScale;
            return orbsInstance;
        }

        private void OnEntityReleased(Entity entity)
        {
            for (int index = _activeBuffs.Count - 1; index >= 0; index--)
            {
                if (_activeBuffs[index].PlantEntity == entity)
                    RemoveBuffAt(index);
            }
        }

        private void RemoveBuffAt(int index)
        {
            ActiveBuff buff = _activeBuffs[index];

            if (buff.OrbsInstance != null)
                Object.Destroy(buff.OrbsInstance);

            _activeBuffs.RemoveAt(index);
            DetachRemainingSecondsReactive(buff.PlantEntity);
        }    }
}
