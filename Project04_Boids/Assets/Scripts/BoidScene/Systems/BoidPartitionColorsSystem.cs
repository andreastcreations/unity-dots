using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ATM.DOTS.Project04
{
    public class BoidPartitionColorsSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            Entities
                .WithName("Colorize_Boid_Partitions")
                .WithAll<BoidTag>()
                .ForEach((ref BasicMaterial_ColorData colorData, in Translation translation) =>
                {
                    int3 key = BoidMovementSystem.GetHashMapKeyFromPosition(translation.Value);

                    if (key.x % 2 == 0)
                    {
                        if (key.y % 2 == 0)
                        {
                            colorData.materialColor = (key.z % 2 == 0) ? new float4(0f, 0f, 1f, 1f) : new float4(1f, 1f, 0f, 1f);
                        }
                        else
                        {
                            colorData.materialColor = (key.z % 2 == 0) ? new float4(1f, 1f, 0f, 1f) : new float4(0f, 0f, 1f, 1f);
                        }
                    }
                    else
                    {
                        if (key.y % 2 == 0)
                        {
                            colorData.materialColor = (key.z % 2 == 0) ? new float4(1f, 1f, 0f, 1f) : new float4(0f, 0f, 1f, 1f);
                        }
                        else
                        {
                            colorData.materialColor = (key.z % 2 == 0) ? new float4(0f, 0f, 1f, 1f) : new float4(1f, 1f, 0f, 1f);
                        }
                    }
                    
                })
                .ScheduleParallel();
        }
    }
}
