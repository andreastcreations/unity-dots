using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;

namespace ATM.DOTS.Project02
{
    // I am just testing the jump input.
    // This is obviously not the best way to implement a jump system.
    public class JumpSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            Entities
                .ForEach((ref PhysicsVelocity velocity,
                          in PhysicsMass mass, in DirectionDataComponent directionData, in MovementDataComponent movementData) =>
                {
                    float3 impulse = new float3(0f, directionData.Value.y * movementData.verticalAcceleration, 0f);
                    if (impulse.y != 0f)
                    {
                        velocity.ApplyLinearImpulse(mass, impulse);
                    }

                    if (velocity.Linear.y > movementData.maxJumpVelocity)
                    {
                        velocity.Linear.y = movementData.maxJumpVelocity;
                    }
                })
                .Run();
        }
    }
}