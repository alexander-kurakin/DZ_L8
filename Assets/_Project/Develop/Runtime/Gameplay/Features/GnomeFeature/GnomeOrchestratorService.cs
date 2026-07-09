using Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomeOrchestratorService : IDisposable
    {
        private readonly EnemiesFactory _enemiesFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly IMouseRaycastService _mouseRaycastService;
        private readonly IMouseInputService _mouseInputService;
        private readonly GnomeArenaConfig _arenaConfig;

        private readonly List<GnomePeekPoint> _peekPoints = new List<GnomePeekPoint>();
        private readonly Dictionary<Entity, GnomePeekPoint> _gnomeToPeekPoint = new Dictionary<Entity, GnomePeekPoint>();
        private readonly Dictionary<Entity, IDisposable> _deathSubscriptions = new Dictionary<Entity, IDisposable>();
        private readonly List<float> _pendingRespawnTimers = new List<float>();
        private readonly List<Entity> _tauntCandidatesBuffer = new List<Entity>();
        private readonly HashSet<Entity> _gazeActivatedGnomes = new HashSet<Entity>();

        private IGnomeGazeScanVisualizer _gazeScanVisualizer;

        private int _remainingGnomesToSpawn;
        private int _totalGnomesKilled;
        private float _gazeCheckElapsedSeconds;
        private bool _isStarted;

        public GnomeOrchestratorService(
            EnemiesFactory enemiesFactory,
            EntitiesLifeContext entitiesLifeContext,
            IMouseRaycastService mouseRaycastService,
            IMouseInputService mouseInputService,
            GnomeArenaConfig arenaConfig)
        {
            _enemiesFactory = enemiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _mouseRaycastService = mouseRaycastService;
            _mouseInputService = mouseInputService;
            _arenaConfig = arenaConfig;
        }

        public int TotalGnomesKilled => _totalGnomesKilled;

        public void SetGazeScanVisualizer(IGnomeGazeScanVisualizer gazeScanVisualizer)
        {
            _gazeScanVisualizer = gazeScanVisualizer;
        }

        public void Initialize(IReadOnlyList<GnomePeekPoint> peekPoints)
        {
            if (peekPoints == null)
                throw new InvalidOperationException("Gnome peek points list is missing.");

            _peekPoints.Clear();
            _peekPoints.AddRange(peekPoints);

            if (_peekPoints.Count == 0)
                Debug.LogWarning("GnomeOrchestratorService: no peek points assigned.");

            _remainingGnomesToSpawn = _arenaConfig.TotalGnomesInRun;
            _totalGnomesKilled = 0;
            _pendingRespawnTimers.Clear();
            _gazeCheckElapsedSeconds = 0f;
        }

        public void StartArena()
        {
            if (_isStarted == true)
                return;

            _isStarted = true;
            SpawnUntilActiveLimit();
        }

        public void Update(float deltaTime)
        {
            if (_isStarted == false)
                return;

            UpdateRespawnTimers(deltaTime);
            UpdateGazeScanVisual();
            UpdateGaze(deltaTime);
        }

        public void Dispose()
        {
            _gazeScanVisualizer?.SetScanContour(Vector3.zero, Vector3.up, 0f, false);

            foreach (KeyValuePair<Entity, IDisposable> subscription in _deathSubscriptions)
                subscription.Value.Dispose();

            _deathSubscriptions.Clear();
            _gnomeToPeekPoint.Clear();
            _gazeActivatedGnomes.Clear();
            _pendingRespawnTimers.Clear();
            _peekPoints.Clear();
        }

        private void SpawnUntilActiveLimit()
        {
            while (_gnomeToPeekPoint.Count < _arenaConfig.MaxActiveGnomes && _remainingGnomesToSpawn > 0)
            {
                if (TrySpawnGnomeAtRandomPeekPoint() == false)
                    break;
            }
        }

        private bool TrySpawnGnomeAtRandomPeekPoint()
        {
            GnomePeekPoint peekPoint = GetRandomFreePeekPoint();

            if (peekPoint == null)
                return false;

            Entity gnome = _enemiesFactory.CreateGnome(peekPoint);

            if (gnome == null)
                return false;

            _remainingGnomesToSpawn--;
            _gnomeToPeekPoint.Add(gnome, peekPoint);

            if (gnome.TryGetComponent(out RunEnemyKillMarker killMarker) == false)
                return false;

            _deathSubscriptions.Add(gnome, killMarker.IsDead.Subscribe((_, isDead) => OnGnomeDied(gnome, isDead)));

            return true;
        }

        private void OnGnomeDied(Entity gnome, bool isDead)
        {
            if (isDead == false)
                return;

            if (_gnomeToPeekPoint.TryGetValue(gnome, out GnomePeekPoint peekPoint) == false)
                return;

            peekPoint.Release();
            _gnomeToPeekPoint.Remove(gnome);
            _gazeActivatedGnomes.Remove(gnome);

            if (_deathSubscriptions.TryGetValue(gnome, out IDisposable subscription) == true)
            {
                subscription.Dispose();
                _deathSubscriptions.Remove(gnome);
            }

            _totalGnomesKilled++;

            float dissolveDelay = gnome.GnomeDeathDissolveDuration;
            DOVirtual.DelayedCall(dissolveDelay, () => _entitiesLifeContext.Release(gnome)).SetUpdate(true).Play();

            if (_remainingGnomesToSpawn > 0)
                _pendingRespawnTimers.Add(_arenaConfig.RespawnDelaySeconds);
        }

        private void UpdateRespawnTimers(float deltaTime)
        {
            for (int timerIndex = _pendingRespawnTimers.Count - 1; timerIndex >= 0; timerIndex--)
            {
                _pendingRespawnTimers[timerIndex] -= deltaTime;

                if (_pendingRespawnTimers[timerIndex] > 0f)
                    continue;

                _pendingRespawnTimers.RemoveAt(timerIndex);
                TrySpawnGnomeAtRandomPeekPoint();
            }
        }

        private void UpdateGazeScanVisual()
        {
            if (_gazeScanVisualizer == null || _mouseRaycastService.Camera == null)
                return;

            Camera camera = _mouseRaycastService.Camera;
            Vector2 screenPosition = _mouseInputService.PointerScreenPosition;

            if (TryGetCoverHit(screenPosition, camera, out RaycastHit hit, out GnomeCoverAnchor _) == false)
            {
                _gazeScanVisualizer.SetScanContour(Vector3.zero, Vector3.up, 0f, false);
                return;
            }

            _gazeScanVisualizer.SetScanContour(hit.point, hit.normal, _arenaConfig.GazeTriggerRadius, true);
        }

        private void UpdateGaze(float deltaTime)
        {
            _gazeCheckElapsedSeconds += deltaTime;

            if (_gazeCheckElapsedSeconds < _arenaConfig.GazeCheckIntervalSeconds)
                return;

            _gazeCheckElapsedSeconds = 0f;

            if (_mouseRaycastService.Camera == null)
                return;

            Camera camera = _mouseRaycastService.Camera;
            Vector2 screenPosition = _mouseInputService.PointerScreenPosition;

            if (TryGetCoverHit(screenPosition, camera, out RaycastHit hit, out GnomeCoverAnchor hitCover) == false)
                return;

            TryTriggerTauntForScan(hitCover, hit.point, hit.normal, _arenaConfig.GazeTriggerRadius);
        }

        private bool TryGetCoverHit(
            Vector2 screenPosition,
            Camera camera,
            out RaycastHit hit,
            out GnomeCoverAnchor coverAnchor)
        {
            coverAnchor = null;

            if (_mouseRaycastService.TryGetHit(
                    screenPosition,
                    out hit,
                    camera.farClipPlane,
                    _arenaConfig.GazeCoverLayerMask) == false)
                return false;

            coverAnchor = hit.collider.GetComponentInParent<GnomeCoverAnchor>();
            return coverAnchor != null;
        }

        private void TryTriggerTauntForScan(GnomeCoverAnchor hitCover, Vector3 scanCenter, Vector3 surfaceNormal, float scanRadius)
        {
            _tauntCandidatesBuffer.Clear();
            Vector3 normalizedSurfaceNormal = surfaceNormal.normalized;

            foreach (KeyValuePair<Entity, GnomePeekPoint> gnomeEntry in _gnomeToPeekPoint)
            {
                Entity gnome = gnomeEntry.Key;
                GnomePeekPoint peekPoint = gnomeEntry.Value;

                if (peekPoint.CoverAnchor != hitCover)
                    continue;

                Vector3 peekFootprintOnSurface = ProjectPointOnPlane(scanCenter, normalizedSurfaceNormal, peekPoint.HiddenPosition);

                if (Vector3.Distance(scanCenter, peekFootprintOnSurface) > scanRadius)
                    continue;

                if (gnome.IsPeeking.Value == true)
                    continue;

                if (gnome.TryGetComponent(out RunEnemyKillMarker killMarker) == false)
                    continue;

                if (killMarker.IsDead.Value == true)
                    continue;

                if (_gazeActivatedGnomes.Contains(gnome) == true)
                    continue;

                _tauntCandidatesBuffer.Add(gnome);
            }

            if (_tauntCandidatesBuffer.Count == 0)
                return;

            Shuffle(_tauntCandidatesBuffer);

            foreach (Entity gnome in _tauntCandidatesBuffer)
            {
                _gazeActivatedGnomes.Add(gnome);
                gnome.GnomeForcePeekRequested.Value = true;
            }

            Debug.Log($"Gnome taunt triggered for {_tauntCandidatesBuffer.Count} gnome(s)");
        }

        private static Vector3 ProjectPointOnPlane(Vector3 planePoint, Vector3 planeNormal, Vector3 point)
        {
            float distanceToPlane = Vector3.Dot(point - planePoint, planeNormal);
            return point - planeNormal * distanceToPlane;
        }

        private static void Shuffle(List<Entity> items)
        {
            for (int itemIndex = items.Count - 1; itemIndex > 0; itemIndex--)
            {
                int swapIndex = Random.Range(0, itemIndex + 1);
                Entity temp = items[itemIndex];
                items[itemIndex] = items[swapIndex];
                items[swapIndex] = temp;
            }
        }

        private GnomePeekPoint GetRandomFreePeekPoint()
        {
            if (_peekPoints.Count == 0)
                return null;

            int startIndex = Random.Range(0, _peekPoints.Count);

            for (int offset = 0; offset < _peekPoints.Count; offset++)
            {
                int pointIndex = (startIndex + offset) % _peekPoints.Count;
                GnomePeekPoint peekPoint = _peekPoints[pointIndex];

                if (peekPoint.IsOccupied == false)
                    return peekPoint;
            }

            return null;
        }
    }
}
