using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public class TowerCenterRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private Transform _spawnPoint;
        public override void Register(Entity entity)
        {
           entity.AddSpawnPoint(_spawnPoint); 
        }
    }
}