using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities
{
    public class Layers
    {
        public static readonly int Characters = LayerMask.NameToLayer("Characters");
        public static readonly LayerMask CharactersMask = 1 << Characters;
        
        public static readonly int Enviroment = LayerMask.NameToLayer("Enviroment");
        public static readonly LayerMask EnviromentMask = 1 << Enviroment;
        
        private static readonly int ContactTrigger = LayerMask.NameToLayer("ContactTrigger");
        public static readonly LayerMask ContactTriggerLayerMask = 1 << ContactTrigger;

        public static readonly LayerMask GenericLayerMask = LayerMask.GetMask("ContactTrigger", "Floor", "Characters");
       
        public static readonly int FloorLayerIndex = LayerMask.NameToLayer("Floor");
        public static readonly LayerMask FloorLayerMask = 1 << FloorLayerIndex;

    }
}
