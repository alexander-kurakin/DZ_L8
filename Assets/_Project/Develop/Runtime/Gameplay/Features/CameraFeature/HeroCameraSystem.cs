using Assets._Project.Develop.Runtime.Configs.Gameplay.Camera;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using DG.Tweening;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.CameraFeature
{
    public class HeroCameraSystem : IInitializableSystem, ILateUpdatableSystem, IDisposableSystem
    {
        private readonly HeroCameraConfig _config;

        private Transform _firstPersonCameraPoint;
        private ReactiveVariable<bool> _isChargingThrow;
        private IDisposable _isChargingThrowSubscription;

        private Camera _camera;
        private Transform _cameraTransform;
        private Vector3 _staticPosition;
        private Quaternion _staticRotation;
        private float _staticFieldOfView;
        private float _modeBlend;
        private Tween _modeBlendTween;

        public HeroCameraSystem(HeroCameraConfig config)
        {
            _config = config;
        }

        public void OnInit(Entity entity)
        {
            _firstPersonCameraPoint = entity.FirstPersonCameraPoint;
            _isChargingThrow = entity.IsChargingThrow;

            _camera = Camera.main;
            _cameraTransform = _camera.transform;
            _staticPosition = _cameraTransform.position;
            _staticRotation = _cameraTransform.rotation;
            _staticFieldOfView = _camera.fieldOfView;

            _isChargingThrowSubscription = _isChargingThrow.Subscribe(OnChargingThrowChanged);
        }

        public void OnLateUpdate(float deltaTime)
        {
            if (_camera == null || _cameraTransform == null)
                return;

            if (_modeBlend <= 0f)
                return;

            Vector3 firstPersonPosition = _firstPersonCameraPoint.position;
            Quaternion firstPersonRotation = _firstPersonCameraPoint.rotation;

            _cameraTransform.position = Vector3.Lerp(_staticPosition, firstPersonPosition, _modeBlend);
            _cameraTransform.rotation = Quaternion.Slerp(_staticRotation, firstPersonRotation, _modeBlend);
            _camera.fieldOfView = Mathf.Lerp(_staticFieldOfView, _config.FirstPersonFieldOfView, _modeBlend);
        }

        public void OnDispose()
        {
            _isChargingThrowSubscription?.Dispose();
            _modeBlendTween?.Kill();
        }

        private void OnChargingThrowChanged(bool oldValue, bool isChargingThrow)
        {
            _modeBlendTween?.Kill();

            float targetBlend = isChargingThrow ? 1f : 0f;

            _modeBlendTween = DOTween
                .To(() => _modeBlend, value => _modeBlend = value, targetBlend, _config.ModeTransitionSeconds)
                .SetEase(Ease.InOutCubic)
                .SetUpdate(true)
                .OnComplete(OnModeBlendTweenCompleted)
                .Play();
        }

        private void OnModeBlendTweenCompleted()
        {
            if (_modeBlend > 0f)
                return;

            _cameraTransform.position = _staticPosition;
            _cameraTransform.rotation = _staticRotation;
            _camera.fieldOfView = _staticFieldOfView;
        }
    }
}
