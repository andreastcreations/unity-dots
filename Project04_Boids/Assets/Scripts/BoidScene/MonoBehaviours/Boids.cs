using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

using Random = Unity.Mathematics.Random;

namespace ATM.DOTS.Project04
{
    /// <summary>
    /// The main component that creates the basic entity archetype of a single boid and spawns the entities based on that archetype.
    /// </summary>
    [RequireComponent(typeof(BoidsColor))]
    [RequireComponent(typeof(BoidsBounds))]
    [RequireComponent(typeof(BoidsMovement))]
    [RequireComponent(typeof(BoidsBehaviours))]
    public class Boids : MonoBehaviour
    {
        [Header("RealTime (Editor Only)")]
        [SerializeField]
        private bool _updateBoidValues = false;

        [Header("Setup")]
        [SerializeField]
        private Mesh _mesh;
        [SerializeField, Min(0f)]
        private float _meshScale = 1f;
        [SerializeField]
        private Material _material;
        [SerializeField, Min(1)]
        private int _boidSize = 1;

        private BoidsColor _boidsColor;
        private BoidsBounds _boidsBounds;
        private BoidsMovement _boidsMovement;
        private BoidsBehaviours _boidsBehaviours;

        private EntityManager _entityManager;
        private Random _random;

        /// <summary>
        /// The parallel job that spawns all the boids.
        /// </summary>
        [BurstCompatible]
        public struct BoidSpawnJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
            public Random entitySeed;
            public Entity entityPrototype;
            public int entityCount;
            public Vector3 spawnBoundsSize;
            public Vector3 spawnerPosition;

            public void Execute(int index)
            {
                Entity entity = entityCommandBuffer.Instantiate(index, entityPrototype);

                entityCommandBuffer.SetComponent(index, entity, new Translation
                {
                    Value = GetSpawnPosition()
                });
                entityCommandBuffer.SetComponent(index, entity, new Rotation
                {
                    Value = GetSpawnRotation()
                });
            }

            private float3 GetSpawnPosition()
            {
                Vector3 insideUnitCircle = new Vector3(entitySeed.NextFloat(-1f, 1f),
                                                       entitySeed.NextFloat(-1f, 1f),
                                                       entitySeed.NextFloat(-1f, 1f));
                Vector3 randomVector = Vector3.Scale(spawnBoundsSize, insideUnitCircle);
                Vector3 spawnPosition = spawnerPosition + randomVector;

                return new float3(spawnPosition.x, spawnPosition.y, spawnPosition.z);
            }

            private quaternion GetSpawnRotation()
            {
                return quaternion.RotateY(entitySeed.NextFloat(-20f, 20f));
            }
        }

        /// <summary>
        /// This parallel job is used to manipulate the boids' shared data at runtime.<br/>
        /// </summary>
        /// <remarks>
        /// WARNING: Used to decide the final values of the boids in the Editor. Not for actual in-game use.
        /// </remarks>
        [BurstCompatible]
        private struct ManipulateBoidDataJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> entities;

            public float4 color;
            public float4 emission;

            public float minForwardSpeed;
            public float maxForwardSpeed;
            public float rotationSpeed;

            public float3 boidBoundsSize;
            public float3 boidBoundsCenter;
            public float partitionsSize;

            public float insideBoundsDistance;
            public float insideBoundsWeight;

            public float cohesionDistance;
            public float cohesionWeight;

            public float avoidanceDistance;
            public float avoidanceWeight;

            public float alignmentDistance;
            public float alignmentWeight;

            public void Execute(int index)
            {
                entityCommandBuffer.SetComponent(index, entities[index], new BasicMaterial_ColorData
                {
                    materialColor = color
                });
                entityCommandBuffer.SetComponent(index, entities[index], new BasicMaterial_EmissionData
                {
                    emissionColor = emission
                });
                entityCommandBuffer.SetSharedComponent(index, entities[index], new BoidSharedData
                {
                    minForwardSpeed = minForwardSpeed,
                    maxForwardSpeed = maxForwardSpeed,
                    rotationSpeed = rotationSpeed,

                    boundsSize = boidBoundsSize,
                    boundsCenter = boidBoundsCenter,
                    partitionsSize = partitionsSize,

                    insideBoundsDistance = insideBoundsDistance,
                    insideBoundsWeight = insideBoundsWeight,

                    cohesionDistance = cohesionDistance,
                    cohesionWeight = cohesionWeight,

                    avoidanceDistance = avoidanceDistance,
                    avoidanceWeight = avoidanceWeight,

                    alignmentDistance = alignmentDistance,
                    alignmentWeight = alignmentWeight
                });
            }
        }

        private void Awake()
        {
            _boidsColor = GetComponent<BoidsColor>();
            _boidsMovement = GetComponent<BoidsMovement>();
            _boidsBounds = GetComponent<BoidsBounds>();
            _boidsBehaviours = GetComponent<BoidsBehaviours>();

            _random = new Random(100);
        }

        private void Start()
        {
            SpawnBoids();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (_updateBoidValues)
            {
                UpdateBoidsInRealTime();
            }
        }
#endif

        /// <summary>
        /// Creates the entity archetype of a boid and spawns a whole bunch of them using the <see cref="BoidSpawnJob"/>.
        /// </summary>
        private void SpawnBoids()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            EntityArchetype entityArchetype = _entityManager.CreateArchetype(
                // Built-in
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(LocalToWorld),
                // Custom
                typeof(BasicMaterial_ColorData),
                typeof(BasicMaterial_SmoothnessData),
                typeof(BasicMaterial_AmbientOcclusionData),
                typeof(BasicMaterial_EmissionData),
                typeof(BoidSharedData)
            );

            Entity entityPrototype = _entityManager.CreateEntity(entityArchetype);
#if UNITY_EDITOR
            _entityManager.SetName(entityPrototype, "Boid");
#endif

            RenderMeshUtility.AddComponents(
                entityPrototype,
                _entityManager,
                new RenderMeshDescription(mesh: _mesh,
                                          material: _material,
                                          subMeshIndex: 0,
                                          layer: 0,
                                          shadowCastingMode: UnityEngine.Rendering.ShadowCastingMode.On,
                                          receiveShadows: true)
                );

            _entityManager.AddComponentData(entityPrototype, new Scale
            {
                Value = _meshScale
            });
            _entityManager.AddComponentData(entityPrototype, new BasicMaterial_ColorData
            {
                materialColor = ColorToFloat4(_boidsColor.Color)
            });
            _entityManager.AddComponentData(entityPrototype, new BasicMaterial_SmoothnessData
            {
                smoothness = 0.2f
            });
            _entityManager.AddComponentData(entityPrototype, new BasicMaterial_AmbientOcclusionData
            {
                ambientOcclusion = 0.2f
            });
            _entityManager.AddComponentData(entityPrototype, new BasicMaterial_EmissionData
            {
                emissionColor = ColorToFloat4(_boidsColor.Emission)
            });
            _entityManager.AddSharedComponentData(entityPrototype, new BoidSharedData
            {
                minForwardSpeed = _boidsMovement.MinSpeed,
                maxForwardSpeed = _boidsMovement.MaxSpeed,
                rotationSpeed = _boidsMovement.RotationSpeed,

                boundsSize = _boidsBounds.Size,
                boundsCenter = _boidsBounds.Center,
                partitionsSize = _boidsBounds.PartitionsSize,

                insideBoundsDistance = _boidsBehaviours.InsideBounds.distance,
                insideBoundsWeight = _boidsBehaviours.InsideBounds.weight,

                cohesionDistance = _boidsBehaviours.Cohesion.distance,
                cohesionWeight = _boidsBehaviours.Cohesion.weight,

                avoidanceDistance = _boidsBehaviours.Avoidance.distance,
                avoidanceWeight = _boidsBehaviours.Avoidance.weight,

                alignmentDistance = _boidsBehaviours.Alignment.distance,
                alignmentWeight = _boidsBehaviours.Alignment.weight
            });

            BoidSpawnJob boidSpawnJob = new BoidSpawnJob
            {
                entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                entitySeed = _random,
                entityPrototype = entityPrototype,
                entityCount = _boidSize,
                spawnBoundsSize = _boidsBounds.Size,
                spawnerPosition = _boidsBounds.Center
            };

            JobHandle spawnHandle = boidSpawnJob.Schedule(_boidSize, 128);
            spawnHandle.Complete();
            
            entityCommandBuffer.Playback(_entityManager);
            entityCommandBuffer.Dispose();
            _entityManager.DestroyEntity(entityPrototype);
        }

        /// <summary>
        /// Updates the boids' shared data in realtime by using the <see cref="ManipulateBoidDataJob"/>.<br/>
        /// </summary>
        /// <remarks>
        /// WARNING: Used to decide the final values of the boids in the Editor. Not for actual in-game use.
        /// </remarks>
        private void UpdateBoidsInRealTime()
        {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            ManipulateBoidDataJob manipulateBoidDataJob = new ManipulateBoidDataJob
            {
                entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                entities = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BoidSharedData>()).ToEntityArray(Allocator.TempJob),

                color = ColorToFloat4(_boidsColor.Color),
                emission = ColorToFloat4(_boidsColor.Emission),

                minForwardSpeed = _boidsMovement.MinSpeed,
                maxForwardSpeed = _boidsMovement.MaxSpeed,
                rotationSpeed = _boidsMovement.RotationSpeed,

                boidBoundsSize = _boidsBounds.Size,
                boidBoundsCenter = _boidsBounds.Center,
                partitionsSize = _boidsBounds.PartitionsSize,

                insideBoundsDistance = _boidsBehaviours.InsideBounds.distance,
                insideBoundsWeight = _boidsBehaviours.InsideBounds.weight,

                cohesionDistance = _boidsBehaviours.Cohesion.distance,
                cohesionWeight = _boidsBehaviours.Cohesion.weight,

                avoidanceDistance = _boidsBehaviours.Avoidance.distance,
                avoidanceWeight = _boidsBehaviours.Avoidance.weight,

                alignmentDistance = _boidsBehaviours.Alignment.distance,
                alignmentWeight = _boidsBehaviours.Alignment.weight
            };

            JobHandle manipulationHandle = manipulateBoidDataJob.Schedule(_boidSize, 128);
            manipulationHandle.Complete();

            entityCommandBuffer.Playback(_entityManager);
            entityCommandBuffer.Dispose();
        }

        /// <summary>
        /// Transforms the RGBA values of a <see cref="Color"/> to a <see cref="float4"/>.
        /// </summary>
        private float4 ColorToFloat4(Color color)
        {
            return new float4(color.r, color.g, color.b, color.a);
        }
    }
}
