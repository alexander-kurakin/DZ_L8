using Assets._Project.Develop.Runtime.Configs.Gameplay.Juice;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature
{
    public class ScreenShakeService
    {
        private const float VERTICAL_SHAKE_STRENGTH_FACTOR = 0.2f;

        private readonly GameplayVfxConfig _config;

        private Camera _camera;
        private Tween _activeShakeTween;

        public ScreenShakeService(GameplayVfxConfig config)
        {
            _config = config;
        }

        public void PlaySmall()
        {
            Play(_config.SmallScreenShakeDurationSeconds, _config.SmallScreenShakeStrength);
        }

        public void PlayMedium()
        {
            Play(_config.MediumScreenShakeDurationSeconds, _config.MediumScreenShakeStrength);
        }

        private void Play(float durationSeconds, float strength)
        {
            if (durationSeconds <= 0f || strength <= 0f)
                return;

            Camera camera = ResolveCamera();

            if (camera == null)
                return;

            _activeShakeTween?.Kill();

            Vector3 shakeStrength = new Vector3(
                strength,
                strength * VERTICAL_SHAKE_STRENGTH_FACTOR,
                strength);

            _activeShakeTween = camera.transform
                .DOShakePosition(
                    durationSeconds,
                    shakeStrength,
                    _config.ScreenShakeVibrato,
                    _config.ScreenShakeRandomness,
                    false,
                    true)
                .Play();
        }

        private Camera ResolveCamera()
        {
            if (_camera != null)
                return _camera;

            _camera = Camera.main;
            return _camera;
        }
    }
}
