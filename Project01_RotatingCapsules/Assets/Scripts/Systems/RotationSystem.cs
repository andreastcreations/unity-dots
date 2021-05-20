using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ATM.DOTS.Project01
{
    public class RotationSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            Entities
                .ForEach((ref RotationDataComponent rotationData, ref Rotation rotation) =>
                {
                    rotation = new Rotation
                    {
                        Value = math.mul(rotation.Value,
                                         quaternion.RotateX(math.radians(rotationData.rotationSpeed * deltaTime)))
                    };
                });
        }
    }
}
