using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public interface IMouseInputService
    {
        bool IsEnabled { get; set; }
        float HorizontalDelta { get; }
        bool FireButtonDown { get; }
        bool FireButtonHeld { get; }
        bool FireButtonUp { get; }
        Vector2 PointerScreenPosition { get; }
        void HideCursor();
        void ShowCursor();
    }
}