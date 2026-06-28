using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public readonly struct SectorId : IEquatable<SectorId>
    {
        public const int SectorsPerRing = 16;

        public SectorBelt Belt { get; }

        public int Index { get; }

        public SectorId(SectorBelt belt, int index)
        {
            if (index < 0 || index >= SectorsPerRing)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Sector index must be in [0, {SectorsPerRing}).");

            Belt = belt;
            Index = index;
        }

        public int BeltPriority => (int)Belt;

        public bool Equals(SectorId other) => Belt == other.Belt && Index == other.Index;

        public override bool Equals(object obj) => obj is SectorId other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Belt, Index);

        public override string ToString() => $"{Belt}:{Index}";

        public static bool operator ==(SectorId left, SectorId right) => left.Equals(right);

        public static bool operator !=(SectorId left, SectorId right) => left.Equals(right) == false;
    }
}
