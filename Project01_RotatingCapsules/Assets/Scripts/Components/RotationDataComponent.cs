using Unity.Entities;

namespace ATM.DOTS.Project01
{
    [GenerateAuthoringComponent]
    public struct RotationDataComponent : IComponentData
    {
        public float rotationSpeed;
    }
}