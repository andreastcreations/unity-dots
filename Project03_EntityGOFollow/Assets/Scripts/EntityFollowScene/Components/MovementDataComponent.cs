using Unity.Entities;

namespace ATM.DOTS.Project03
{
    [GenerateAuthoringComponent]
    public struct MovementDataComponent : IComponentData
    {
        public float horizontalAcceleration;
    }
}