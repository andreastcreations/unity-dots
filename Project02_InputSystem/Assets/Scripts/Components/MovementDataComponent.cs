using Unity.Entities;

namespace ATM.DOTS.Project02
{
    [GenerateAuthoringComponent]
    public struct MovementDataComponent : IComponentData
    {
        public float horizontalAcceleration;
        public float verticalAcceleration;
        public float maxJumpVelocity;
    }
}