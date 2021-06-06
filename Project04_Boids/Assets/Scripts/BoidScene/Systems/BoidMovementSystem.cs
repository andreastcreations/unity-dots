using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ATM.DOTS.Project04
{
    /// <summary>
    /// The system responsible for the movement of all boids.
    /// </summary>
    public class BoidMovementSystem : SystemBase
    {
        private List<BoidSharedData> _uniqueBoidGroups = new List<BoidSharedData>();
        private EntityQuery _boidQuery;

        /// <summary>
        /// Stores the position and the forward vector of an entity.
        /// </summary>
        private struct EntityData
        {
            public float3 entityPosition;
            public float3 entityForward;
        }

        protected override void OnCreate()
        {
            _boidQuery = GetEntityQuery(ComponentType.ReadOnly<BoidSharedData>(), ComponentType.ReadWrite<LocalToWorld>());
            RequireForUpdate(_boidQuery);
        }

        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(_uniqueBoidGroups);

            for (int boidGroup = 0; boidGroup < _uniqueBoidGroups.Count; boidGroup++)
            {
                BoidSharedData boidData = _uniqueBoidGroups[boidGroup];
                _boidQuery.AddSharedComponentFilter(boidData);

                int boidCount = _boidQuery.CalculateEntityCount();
                if (boidCount == 0)
                {
                    _boidQuery.ResetFilter();
                    continue;
                }

                float deltaTime = Time.DeltaTime;

                NativeMultiHashMap<int3, EntityData> boidPartitions = new NativeMultiHashMap<int3, EntityData>(boidCount, Allocator.TempJob);
                NativeArray<Entity> entitiesFromQuery = _boidQuery.ToEntityArray(Allocator.TempJob);

                NativeMultiHashMap<int3, EntityData>.ParallelWriter boidPartitionsParallel = boidPartitions.AsParallelWriter();
                Entities
                    .WithName("CreatePartitions_ForEach")
                    .WithSharedComponentFilter(boidData)
                    .ForEach((in LocalToWorld localToWorld) =>
                    {
                        int3 hashKey = GetHashMapKeyFromPosition(localToWorld.Position, boidData.partitionsSize);

                        boidPartitionsParallel.Add(hashKey, new EntityData
                        {
                            entityPosition = localToWorld.Position,
                            entityForward = localToWorld.Forward
                        });
                    })
                    .ScheduleParallel();

                Entities
                    .WithName("CombinedMovement_ForEach")
                    .WithSharedComponentFilter(boidData)
                    .WithReadOnly(boidPartitions)
                    .ForEach((ref Translation translation, ref Rotation rotation,
                              in LocalToWorld localToWorld) =>
                    {
                        float3 currentPosition = localToWorld.Position;
                        float3 currentForward = localToWorld.Forward;
                        int3 hashKey = GetHashMapKeyFromPosition(currentPosition, boidData.partitionsSize);
                        int cohesionTotalBoids = 0;
                        int avoidanceTotalBoids = 0;
                        int alignmentTotalBoids = 0;
                        float3 cohesion = float3.zero;
                        float3 avoidance = float3.zero;
                        float3 alignment = currentForward;
                        float3 reCenter = float3.zero;
                        float3 finalMovement = float3.zero;

                        if (boidPartitions.TryGetFirstValue(hashKey, out EntityData entityData, out NativeMultiHashMapIterator<int3> iterator))
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

                            } while (boidPartitions.TryGetNextValue(out entityData, ref iterator));

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

                            if (math.length(boidData.boundsCenter - currentPosition) / boidData.insideBoundsDistance > 0.9f)
                            {
                                reCenter = boidData.boundsCenter - currentPosition;
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
                            rotation.Value = math.slerp(rotation.Value,
                                                        quaternion.LookRotationSafe(finalMovement, localToWorld.Up),
                                                        deltaTime * boidData.rotationSpeed);
                        }
                    })
                    .ScheduleParallel();

                JobHandle disposeJobHandle = boidPartitions.Dispose(Dependency);
                disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, entitiesFromQuery.Dispose(Dependency));
                Dependency = disposeJobHandle;

                _boidQuery.AddDependency(Dependency);
                _boidQuery.ResetFilter();
            }
            _uniqueBoidGroups.Clear();
        }

        public static int3 GetHashMapKeyFromPosition(float3 position, float partitionsSize)
        {
            return new int3((int)math.floor(position.x / partitionsSize),
                            (int)math.floor(position.y / partitionsSize),
                            (int)math.floor(position.z / partitionsSize));
        }
    }
}