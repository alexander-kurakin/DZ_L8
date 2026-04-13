using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public interface IMouseInputService
    {
        bool IsEnabled { get; set; }
        float HorizontalDelta { get; }
        bool FireButtonPressed { get; }
        Vector2 PointerScreenPosition { get; }
    }
}