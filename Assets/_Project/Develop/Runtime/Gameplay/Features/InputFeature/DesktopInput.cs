using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class DesktopInput : IInputService
    {
        private const string HorizontalAxisName = "Horizontal";
        private const string VerticalAxisName = "Vertical";

        public bool IsEnabled { get; set; } = true;

        public Vector3 Direction
        {
            get
            {
                if (IsEnabled == false)
                    return Vector3.zero;

                float horizontal = Input.GetAxisRaw(HorizontalAxisName);
                float vertical = Input.GetAxisRaw(VerticalAxisName);

                if (horizontal == 0f && vertical == 0f)
                    return Vector3.zero;

                Camera camera = Camera.main;

                if (camera == null)
                    return new Vector3(horizontal, 0f, vertical);

                Vector3 cameraForward = camera.transform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                Vector3 cameraRight = camera.transform.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                return (cameraForward * vertical + cameraRight * horizontal).normalized;
            }
        }
    }
}
