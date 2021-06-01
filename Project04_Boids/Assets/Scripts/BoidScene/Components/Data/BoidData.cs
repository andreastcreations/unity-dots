using Unity.Entities;
using Unity.Mathematics;

namespace ATM.DOTS.Project04
{
    public struct BoidData : IComponentData
    {
        public float minForwardSpeed;
        public float maxForwardSpeed;
        public float rotationSpeed;

        public float3 boidBoundsSize;
        public float3 boidBoundsCenter;
        public float insideBoundsDistance;
        public float insideBoundsWeight;

        public float cohesionDistance;
        public float cohesionWeight;

        public float avoidanceDistance;
        public float avoidanceWeight;

        public float alignmentDistance;
        public float alignmentWeight;
    }
}
