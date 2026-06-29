using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public class MineIdleVisualView : EntityView
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Vector3 _localScaleMultiplier = new Vector3(2f, 1f, 2f);

        protected override void OnEntityStartedWork(Entity entity)
        {
            Transform visualRoot = ResolveVisualRoot();
            visualRoot.localScale = _localScaleMultiplier;
            GameplayVfxUtility.PlayLoopingParticleSystemsInChildren(visualRoot.gameObject);
        }

        private Transform ResolveVisualRoot()
        {
            if (_visualRoot == null)
                return transform;

            if (_visualRoot.childCount == 0)
                return _visualRoot;

            return _visualRoot.GetChild(0);
        }
    }
}
