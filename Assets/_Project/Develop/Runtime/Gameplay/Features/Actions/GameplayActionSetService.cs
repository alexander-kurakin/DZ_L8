using System;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Actions
{
    public class GameplayActionSetService : IDisposable
    {
        private readonly ReactiveVariable<ActionSet> _currentActionSet = new(ActionSet.Peaceful);
        
        public IReadOnlyVariable<ActionSet> CurrentActionSet => _currentActionSet;
        
        private IDisposable _currentActionSetRequest;

        public GameplayActionSetService()
        {
            _currentActionSetRequest = _currentActionSet.Subscribe(OnActionSetChanged);
        }

        private void OnActionSetChanged(ActionSet arg1, ActionSet arg2)
        {
            Debug.Log($"{nameof(OnActionSetChanged)}({arg1}, {arg2})");
        }

        public void SetActionSet(ActionSet actionSet)
        {
            _currentActionSet.Value = actionSet;
        }
        
        public void Dispose()
        {
            _currentActionSetRequest?.Dispose();
        }
    }
}