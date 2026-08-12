using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.CoachHintArrows
{
    public class CoachHintArrowView : MonoBehaviour, IView
    {
        public const float PointDownZDegrees = 180f;
        public const float PointDownDiagonalZDegrees = 135f;

        [SerializeField] private Image _icon;
        [SerializeField] private RectTransform _rectTransform;

        public RectTransform RectTransform => _rectTransform;

        public void SetIcon(Sprite icon)
        {
            _icon.sprite = icon;
            _icon.preserveAspect = true;
        }

        public void SetColor(Color color)
        {
            _icon.color = color;
        }

        public void SetRotationZ(float zDegrees)
        {
            _rectTransform.localEulerAngles = new Vector3(0f, 0f, zDegrees);
        }
    }
}
