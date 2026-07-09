using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomeCoverAnchor : MonoBehaviour
    {
        public const float DefaultGazeRadiusGizmo = 1.25f;

        [SerializeField] private Collider _coverCollider;
        [SerializeField] private float _gazeRadiusGizmo = DefaultGazeRadiusGizmo;

        public Collider CoverCollider => _coverCollider;
        public float GazeRadiusGizmo => _gazeRadiusGizmo;
    }
}
