using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ATM.DOTS.Project04
{
    public class BoidMovementSystem : SystemBase
    {
        private const int _partitionSize = 5;

        private static NativeMultiHashMap<int3, EntityData> _boidPartitions;

        private EntityQuery _boidQuery;

        private struct EntityData
        {
            public float3 entityPosition;
            public float3 entityForward;
        }

        protected override void OnCreate()
        {
            _boidPartitions = new NativeMultiHashMap<int3, EntityData>(0, Allocator.Persistent);
            _boidQuery = GetEntityQuery(typeof(BoidTag));
        }
        protected override void OnDestroy() => _boidPartitions.Dispose();

        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            int boidCount = _boidQuery.CalculateEntityCount();
            _boidPartitions.Clear();

            if (_boidPartitions.Capacity < boidCount)
            {
                _boidPartitions.Capacity = boidCount;
            }

            NativeMultiHashMap<int3, EntityData>.ParallelWriter boidPartitionsParallel = _boidPartitions.AsParallelWriter();
            NativeArray<Entity> entitiesFromQuery = _boidQuery.ToEntityArray(Allocator.TempJob);

            Entities
                .WithName("CreatePartitions_ForEach")
                .WithAll<BoidTag>()
                .ForEach((in LocalToWorld localToWorld) =>
                {
                    int3 hashKey = GetHashMapKeyFromPosition(localToWorld.Position);

                    boidPartitionsParallel.Add(hashKey, new EntityData
                    {
                        entityPosition = localToWorld.Position,
                        entityForward = localToWorld.Forward
                    });
                })
                .ScheduleParallel();

            NativeMultiHashMap<int3, EntityData> boidPartitionsForJob = _boidPartitions;

            Entities
                .WithName("CombinedMovement_ForEach")
                .WithAll<BoidTag>()
                .WithReadOnly(boidPartitionsForJob)
                .ForEach((ref Translation translation, ref Rotation rotation,
                          in LocalToWorld localToWorld, in BoidData boidData) =>
                {
                    float3 currentPosition = translation.Value;
                    float3 currentForward = localToWorld.Forward;
                    int3 hashKey = GetHashMapKeyFromPosition(currentPosition);
                    int cohesionTotalBoids = 0;
                    int avoidanceTotalBoids = 0;
                    int alignmentTotalBoids = 0;
                    float3 cohesion = float3.zero;
                    float3 avoidance = float3.zero;
                    float3 alignment = localToWorld.Forward;
                    float3 reCenter = float3.zero;
                    float3 finalMovement = float3.zero;

                    if (boidPartitionsForJob.TryGetFirstValue(hashKey, out EntityData entityData, out NativeMultiHashMapIterator<int3> iterator))
                    {
                        do
                        {
                            if (!currentPosition.Equals(entityData.entityPosition))
                            {
                                float distance = math.distance(currentPosition, entityData.entityPosition);

                                if (distance < boidData.cohesionDistance)
                                {
                                    cohesion += entityData.entityPosition;
                                    cohesionTotalBoids++;
                                }
                                if (distance < boidData.avoidanceDistance)
                                {
                                    avoidance += currentPosition - entityData.entityPosition;
                                    avoidanceTotalBoids++;
                                }
                                if (distance < boidData.alignmentDistance)
                                {
                                    alignment += entityData.entityForward;
                                    alignmentTotalBoids++;
                                }
                            }

                        } while (boidPartitionsForJob.TryGetNextValue(out entityData, ref iterator));

                        if (cohesionTotalBoids > 0)
                        {
                            cohesion /= cohesionTotalBoids;
                            cohesion -= currentPosition;
                            cohesion = math.normalizesafe(cohesion) * boidData.cohesionWeight;
                        }

                        if (avoidanceTotalBoids > 0)
                        {
                            avoidance /= avoidanceTotalBoids;
                            avoidance = math.normalizesafe(avoidance) * boidData.avoidanceWeight;
                        }

                        if (alignmentTotalBoids > 0)
                        {
                            alignment /= alignmentTotalBoids;
                            alignment = math.normalizesafe(alignment) * boidData.alignmentWeight;
                        }

                        if (math.length(boidData.boidBoundsCenter - currentPosition) / boidData.insideBoundsDistance > 0.9f)
                        {
                            reCenter = boidData.boidBoundsCenter - currentPosition;
                            reCenter = math.normalizesafe(reCenter) * boidData.insideBoundsWeight;
                        }

                        finalMovement = cohesion + alignment + avoidance + reCenter;
                        if (math.length(finalMovement) < boidData.minForwardSpeed)
                        {
                            finalMovement = math.normalizesafe(finalMovement) * boidData.minForwardSpeed;
                        }
                        else if (math.length(finalMovement) > boidData.maxForwardSpeed)
                        {
                            finalMovement = math.normalizesafe(finalMovement) * boidData.maxForwardSpeed;
                        }

                        if (finalMovement.Equals(float3.zero))
                        {
                            finalMovement = localToWorld.Forward;
                        }

                        translation.Value += finalMovement * deltaTime;
                        rotation.Value = math.slerp(rotation.Value, quaternion.LookRotationSafe(finalMovement, localToWorld.Up), deltaTime);
                    }
                })
                .ScheduleParallel();

            JobHandle nativeDisposeHandle = entitiesFromQuery.Dispose(Dependency);
            Dependency = nativeDisposeHandle;
        }

        public static int3 GetHashMapKeyFromPosition(float3 position)
        {
            return new int3((int)math.floor(position.x / _partitionSize),
                            (int)math.floor(position.y / _partitionSize),
                            (int)math.floor(position.z / _partitionSize));
        }
    }
}