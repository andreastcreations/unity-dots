using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ATM.DOTS.Project02
{
    public class HorizontalMovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            Entities
                .ForEach((ref Translation translation,
                          in DirectionDataComponent directionData, in MovementDataComponent movementData) =>
                {
                    translation.Value += new float3(directionData.Value.x * movementData.horizontalAcceleration * deltaTime,
                                                    0f,
                                                    directionData.Value.z * movementData.horizontalAcceleration * deltaTime);
                })
                .Run();
        }
    }
}