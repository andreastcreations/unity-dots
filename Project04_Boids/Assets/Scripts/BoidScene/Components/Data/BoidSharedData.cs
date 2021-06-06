using Unity.Entities;
using Unity.Mathematics;

namespace ATM.DOTS.Project04
{
    /// <summary>
    /// Stores common data of a boid group.
    /// </summary>
    public struct BoidSharedData : ISharedComponentData
    {
        public float minForwardSpeed;
        public float maxForwardSpeed;
        public float rotationSpeed;

        public float3 boundsSize;
        public float3 boundsCenter;
        public float partitionsSize;

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
