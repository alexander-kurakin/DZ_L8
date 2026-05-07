using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Meta.Features.Powerups
{
    public class PowerupList
    {
        public event Action<Powerup> Added;

        private List<Powerup> _elements = new();

        public IReadOnlyList<Powerup> Elements => _elements;

        public virtual void Add(Powerup element)
        {
            _elements.Add(element);
            Added?.Invoke(element);
        }
    }
}