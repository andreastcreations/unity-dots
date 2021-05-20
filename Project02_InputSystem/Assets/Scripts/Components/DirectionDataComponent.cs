using Unity.Entities;
using Unity.Mathematics;

namespace ATM.DOTS.Project02
{
    [GenerateAuthoringComponent]
    public struct DirectionDataComponent : IComponentData
    {
        public float3 Value;
    }
}