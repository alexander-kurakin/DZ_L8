using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono
{
    public abstract class EntityView : MonoBehaviour
    {
        protected IGameSoundsService GameSoundsesService { get;  private set; }
        
        public void Link(Entity entity, IGameSoundsService gameSoundsService)
        {
            GameSoundsesService = gameSoundsService;
            entity.Initialized += OnEntityStartedWork;
        }

        public virtual void Cleanup(Entity entity)
        {
            entity.Initialized -= OnEntityStartedWork;
        }

        protected abstract void OnEntityStartedWork(Entity entity);
    }
}
