using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public class ThrowTrajectoryPreviewSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly IMouseInputService _mouseInput;
        private readonly IMouseRaycastService _mouseRaycastService;
        private readonly ThrowChargeConfig _throwChargeConfig;
        private readonly List<Vector3> _predictedPointsBuffer = new List<Vector3>();

        private ReactiveVariable<bool> _isChargingThrow;
        private ReactiveVariable<float> _throwChargePower;
        private Transform _heroTransform;
        private Transform _throwReleasePoint;
        private ReactiveVariable<bool> _isVisible;
        private List<ReactiveVariable<Vector3>> _trajectoryPoints;

        public ThrowTrajectoryPreviewSystem(
            IMouseInputService mouseInput,
            IMouseRaycastService mouseRaycastService,
            ThrowChargeConfig throwChargeConfig)
        {
            _mouseInput = mouseInput;
            _mouseRaycastService = mouseRaycastService;
            _throwChargeConfig = throwChargeConfig;
        }

        public void OnInit(Entity entity)
        {
            _isChargingThrow = entity.IsChargingThrow;
            _throwChargePower = entity.ThrowChargePower;
            _heroTransform = entity.Transform;
            _throwReleasePoint = entity.ThrowReleasePoint;

            ThrowTrajectoryPreview trajectoryPreview = entity.GetComponent<ThrowTrajectoryPreview>();
            _isVisible = trajectoryPreview.IsVisible;
            _trajectoryPoints = trajectoryPreview.TrajectoryPoints;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_isChargingThrow.Value == false)
            {
                _isVisible.Value = false;
                _trajectoryPoints.Clear();
                return;
            }

            if (_throwReleasePoint == null)
            {
                _isVisible.Value = false;
                _trajectoryPoints.Clear();
                return;
            }

            float power = _throwChargePower.Value;
            Vector2 pointerScreenPosition = _mouseInput.PointerScreenPosition;

            if (_mouseRaycastService.TryGetThrowDirectionFromCamera(pointerScreenPosition, out Vector3 cameraDirection) == false)
            {
                _isVisible.Value = false;
                _trajectoryPoints.Clear();
                return;
            }

            Vector3 direction = _throwChargeConfig.ResolveThrowDirection(cameraDirection, power);

            float speed = _throwChargeConfig.EvaluateThrowSpeed(power);

            if (speed <= 0f)
            {
                _isVisible.Value = false;
                _trajectoryPoints.Clear();
                return;
            }

            ThrowTrajectoryPredictionUtility.Predict(
                _predictedPointsBuffer,
                _throwReleasePoint.position,
                direction,
                speed,
                _heroTransform,
                _throwChargeConfig.TrajectoryPreviewMaxPoints);

            SyncTrajectoryPoints(_predictedPointsBuffer);
            _isVisible.Value = _trajectoryPoints.Count >= 2;
        }

        private void SyncTrajectoryPoints(List<Vector3> predictedPoints)
        {
            while (_trajectoryPoints.Count > predictedPoints.Count)
                _trajectoryPoints.RemoveAt(_trajectoryPoints.Count - 1);

            for (int pointIndex = 0; pointIndex < predictedPoints.Count; pointIndex++)
            {
                if (pointIndex >= _trajectoryPoints.Count)
                {
                    _trajectoryPoints.Add(new ReactiveVariable<Vector3>(predictedPoints[pointIndex]));
                    continue;
                }

                _trajectoryPoints[pointIndex].Value = predictedPoints[pointIndex];
            }
        }
    }
}
