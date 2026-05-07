using System;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Meta.Features.Powerups
{
    public abstract class Powerup
    {
        private ReactiveVariable<int> _currentLevel;

        protected Powerup(string id, int currentLevel, int maxLevel)
        {
            ID = id;
            MaxLevel = maxLevel;
            _currentLevel = new ReactiveVariable<int>(currentLevel);
        }

        public string ID { get; }
        public int MaxLevel { get; }

        public IReadOnlyVariable<int> CurrentLevel => _currentLevel;

        public void AddLevel(int level)
        {
            int temp = _currentLevel.Value + level;

            if (temp > MaxLevel)
                throw new ArgumentException(nameof(level));

            _currentLevel.Value = temp;
        }

        public abstract void Activate();
    }
}