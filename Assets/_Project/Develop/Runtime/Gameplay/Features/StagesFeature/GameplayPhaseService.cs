using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class GameplayPhaseService
    {
        private readonly ReactiveVariable<GameplayStates> _currentPhase = new(GameplayStates.Search);

        public IReadOnlyVariable<GameplayStates> CurrentPhase => _currentPhase;

        public void SetPhase(GameplayStates phase)
        {
            _currentPhase.Value = phase;
        }
    }
}
