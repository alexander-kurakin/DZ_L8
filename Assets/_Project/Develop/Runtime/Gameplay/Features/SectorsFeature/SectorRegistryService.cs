using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SectorRegistryService
    {
        private readonly Dictionary<SectorId, Vector3> _anchorBySector = new();
        private readonly Dictionary<Collider, SectorId> _sectorByCollider = new();
        private readonly List<int> _unlockedPathIndices = new();

        private Vector3 _center;
        private SectorGridConfig _gridConfig;
        private bool _isInitialized;

        public Vector3 Center => _center;

        public SectorGridConfig GridConfig => _gridConfig;

        public IReadOnlyList<int> UnlockedPathIndices => _unlockedPathIndices;

        public bool IsInitialized => _isInitialized;

        public void InitializeGrid(Vector3 center, SectorGridConfig gridConfig)
        {
            _center = center;
            _gridConfig = gridConfig;
            _anchorBySector.Clear();
            _sectorByCollider.Clear();
            _unlockedPathIndices.Clear();

            for (int beltIndex = 0; beltIndex < Enum.GetValues(typeof(SectorBelt)).Length; beltIndex++)
            {
                SectorBelt belt = (SectorBelt)beltIndex;

                for (int sectorIndex = 0; sectorIndex < SectorId.SectorsPerRing; sectorIndex++)
                {
                    SectorId sectorId = new SectorId(belt, sectorIndex);
                    Vector3 anchor = WorldToSector.GetAnchorPosition(center, sectorId, gridConfig);
                    _anchorBySector[sectorId] = anchor;
                }
            }

            _unlockedPathIndices.Clear();
            _isInitialized = true;
        }

        public void SetUnlockedPathIndices(IReadOnlyList<int> pathIndices)
        {
            _unlockedPathIndices.Clear();

            foreach (int pathIndex in pathIndices)
            {
                if (IsValidPathIndex(pathIndex) == false)
                    throw new ArgumentOutOfRangeException(nameof(pathIndices), pathIndex, "Path index out of range.");

                _unlockedPathIndices.Add(pathIndex);
            }
        }

        public bool IsPathUnlocked(int pathIndex)
        {
            for (int index = 0; index < _unlockedPathIndices.Count; index++)
            {
                if (_unlockedPathIndices[index] == pathIndex)
                    return true;
            }

            return false;
        }

        public Vector3 GetPlantAnchor(SectorBelt belt, int pathIndex)
        {
            EnsureInitialized();

            SectorId sectorId = new SectorId(belt, pathIndex);
            return GetAnchorPosition(sectorId);
        }

        public Vector3 GetAnchorPosition(SectorId sectorId)
        {
            EnsureInitialized();

            if (_anchorBySector.TryGetValue(sectorId, out Vector3 anchor))
                return anchor;

            return WorldToSector.GetAnchorPosition(_center, sectorId, _gridConfig);
        }

        public Vector3 GetSpawnPositionInWedge(int pathIndex, float angleOffsetRadians, float radiusScale)
        {
            EnsureInitialized();

            if (IsPathUnlocked(pathIndex) == false)
                throw new InvalidOperationException($"Path {pathIndex} is not unlocked for spawn.");

            return WorldToSector.GetPositionInWedge(_center, pathIndex, _gridConfig, angleOffsetRadians, radiusScale);
        }

        public void RegisterVolume(SectorId sectorId, Vector3 anchorPosition, Collider triggerCollider)
        {
            EnsureInitialized();
            _anchorBySector[sectorId] = anchorPosition;

            if (triggerCollider == null)
                return;

            if (_sectorByCollider.ContainsKey(triggerCollider))
                _sectorByCollider.Remove(triggerCollider);

            _sectorByCollider.Add(triggerCollider, sectorId);
        }

        public void UnregisterVolume(Collider triggerCollider)
        {
            if (triggerCollider == null)
                return;

            _sectorByCollider.Remove(triggerCollider);
        }

        public bool TryResolveSectorFromCollider(Collider collider, out SectorId sectorId)
        {
            if (collider == null)
            {
                sectorId = default;
                return false;
            }

            return _sectorByCollider.TryGetValue(collider, out sectorId);
        }

        public int UnlockedPathCount => _unlockedPathIndices.Count;

        public int GetUnlockedPathIndexAt(int listIndex) => _unlockedPathIndices[listIndex];

        private static bool IsValidPathIndex(int pathIndex) =>
            pathIndex >= 0 && pathIndex < SectorId.SectorsPerRing;

        private void EnsureInitialized()
        {
            if (_isInitialized == false)
                throw new InvalidOperationException($"{nameof(SectorRegistryService)} is not initialized.");
        }
    }
}
