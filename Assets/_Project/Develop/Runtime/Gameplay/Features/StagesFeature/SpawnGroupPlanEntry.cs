using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public readonly struct SpawnGroupPlanEntry
    {
        public SpawnGroupPlanEntry(int pathIndex, WaveEnemyPreviewType previewType)
        {
            PathIndex = pathIndex;
            PreviewType = previewType;
        }

        public int PathIndex { get; }

        public WaveEnemyPreviewType PreviewType { get; }
    }
}
