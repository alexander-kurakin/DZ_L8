using Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GnomePeekPointsHolder
    {
        private IReadOnlyList<GnomePeekPoint> _peekPoints;

        public IReadOnlyList<GnomePeekPoint> PeekPoints => _peekPoints;

        public void Set(IReadOnlyList<GnomePeekPoint> peekPoints)
        {
            _peekPoints = peekPoints;
        }
    }
}
