using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay
{
    public class EntityHealthPresenter : IPresenter
    {
        private BarWithText _bar;
        private Entity _entity;
        private ReactiveVariable<Teams> _team;
        private ReactiveVariable<float> _health;
        private ReactiveVariable<float> _maxHealth;

        private ReactiveVariable<float> _distanceToTargetCurrent;

        private List<IDisposable> _disposables = new();
        private bool _isDebugEnabled = false;

        public EntityHealthPresenter(
            Entity entity,
            BarWithText bar)
        {
            _entity = entity;
            _bar = bar;
        }

        public BarWithText Bar => _bar;

        public void Initialize()
        {
            _health = _entity.CurrentHealth;
            _maxHealth = _entity.MaxHealth;
            _team = _entity.Team;

            InitDebug();

            _disposables.Add(_health.Subscribe(OnHealthChanged));
            _disposables.Add(_maxHealth.Subscribe(OnMaxHealthChanged));
            _disposables.Add(_team.Subscribe(OnTeamChanged));

            UpdateHealth();
            UpdateFillerColorBy(_team.Value);
        }

        private void InitDebug()
        {
            if (_isDebugEnabled == false)
                return;
            
            if (_team.Value != Teams.MainHero)
            {
                _distanceToTargetCurrent = _entity.DistanceToTargetCurrent;
                _disposables.Add(_distanceToTargetCurrent.Subscribe(OnDistanceChanged));
            }
        }

        private void OnDistanceChanged(float arg1, float arg2) => UpdateDistance();

        private void UpdateDistance()
        {
            if (_team.Value != Teams.MainHero)
                _bar.SetDebugText(_distanceToTargetCurrent.Value.ToString());
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();
        }

        private void OnTeamChanged(Teams oldValue, Teams newTeam) => UpdateFillerColorBy(newTeam);

        private void OnMaxHealthChanged(float oldValue, float newValue) => UpdateHealth();

        private void OnHealthChanged(float oldValue, float newValue) => UpdateHealth();
        

        private void UpdateHealth()
        {
            _bar.UpdateText(_health.Value.ToString("0"));
            _bar.UpdateSlider(_health.Value / _maxHealth.Value);
        }

        private void UpdateFillerColorBy(Teams team)
        {
            if (team == Teams.MainHero)
                _bar.SetFillerColor(Color.green);
            else if (team == Teams.Enemies)
                _bar.SetFillerColor(Color.red);
        }
    }
}
