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
    public class Boids : MonoBehaviour
    {
        [Header("RealTime")]
        [SerializeField]
        private bool _updateBoidValues = false;

        [Header("Spawn Setup")]
        [SerializeField]
        private Mesh _mesh;
        [SerializeField, Min(0f)]
        private float _meshScale = 1f;
        [SerializeField]
        private Material _material;
        [SerializeField, Min(1)]
        private int _boidSize = 1;

        [Header("Boid Bounds")]
        [SerializeField]
        private Vector3 _spawnBounds;
        [SerializeField]
        private Vector3 _spawnBoundsCenter;
        [Range(0f, 10f)]
        [SerializeField] private float _insideBoundsDistance;
        [Range(0f, 10f)]
        [SerializeField] private float _insideBoundsWeight;

        [Header("Speed Setup")]
        [Range(0f, 10f)]
        [SerializeField] private float _minSpeed;
        [Range(0f, 10f)]
        [SerializeField] private float _maxSpeed;
        [Range(0f, 50f)]
        [SerializeField] private float _rotationSpeed;

        [Header("Boid Cohesion")]
        [Range(0f, 10f)]
        [SerializeField] private float _cohesionDistance;
        [Range(0f, 10f)]
        [SerializeField] private float _cohesionWeight;

        [Header("Boid Avoidance")]
        [Range(0f, 10f)]
        [SerializeField] private float _avoidanceDistance;
        [Range(0f, 10f)]
        [SerializeField] private float _avoidanceWeight;

        [Header("Boid Alignment")]
        [Range(0f, 10f)]
        [SerializeField] private float _aligementDistance;
        [Range(0f, 10f)]
        [SerializeField] private float _aligementWeight;

        private EntityManager _entityManager;
        private Random _random;

        [BurstCompatible]
        public struct BoidSpawnJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
            public Random entitySeed;
            public Entity entityPrototype;
            public int entityCount;
            public Vector3 spawnBounds;
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
                Vector3 randomVector = Vector3.Scale(spawnBounds, insideUnitCircle);
                Vector3 spawnPosition = spawnerPosition + randomVector;

                return new float3(spawnPosition.x, spawnPosition.y, spawnPosition.z);
            }

            private quaternion GetSpawnRotation()
            {
                return quaternion.RotateY(entitySeed.NextFloat(-20f, 20f));
            }
        }

        [BurstCompatible]
        private struct ManipulateBoidDataJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> entities;
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

            public void Execute(int index)
            {
                entityCommandBuffer.SetComponent(index, entities[index], new BoidData
                {
                    minForwardSpeed = minForwardSpeed,
                    maxForwardSpeed = maxForwardSpeed,
                    rotationSpeed = rotationSpeed,

                    boidBoundsSize = boidBoundsSize,
                    boidBoundsCenter = boidBoundsCenter,
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
            _random = new Random(100);
        }

        private void Start()
        {
            SpawnBoids();
        }

        private void Update()
        {
            if (_updateBoidValues)
            {
                UpdateBoidsInRealTime();
            }
        }

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
                // Tags
                typeof(BoidTag),
                // Custom
                typeof(BasicMaterial_ColorData),
                typeof(BasicMaterial_SmoothnessData),
                typeof(BasicMaterial_AmbientOcclusionData),
                typeof(BasicMaterial_EmissionData),
                typeof(BoidData)
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
                materialColor = new float4(1f, 1f, 1f, 1f)
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
                emissionColor = new float4(0.5f, 1f, 0f, 0f)
            });
            _entityManager.AddComponentData(entityPrototype, new BoidData
            {
                minForwardSpeed = _minSpeed,
                maxForwardSpeed = _maxSpeed,
                rotationSpeed = _rotationSpeed,

                boidBoundsSize = _spawnBounds,
                boidBoundsCenter = _spawnBoundsCenter,
                insideBoundsDistance = _insideBoundsDistance,
                insideBoundsWeight = _insideBoundsWeight,

                cohesionDistance = _cohesionDistance,
                cohesionWeight = _cohesionWeight,

                avoidanceDistance = _avoidanceDistance,
                avoidanceWeight = _avoidanceWeight,

                alignmentDistance = _aligementDistance,
                alignmentWeight = _aligementWeight
            });

            BoidSpawnJob boidSpawnJob = new BoidSpawnJob
            {
                entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                entitySeed = _random,
                entityPrototype = entityPrototype,
                entityCount = _boidSize,
                spawnBounds = _spawnBounds,
                spawnerPosition = transform.position
            };

            JobHandle spawnHandle = boidSpawnJob.Schedule(_boidSize, 128);
            spawnHandle.Complete();
            
            entityCommandBuffer.Playback(_entityManager);
            entityCommandBuffer.Dispose();
            _entityManager.DestroyEntity(entityPrototype);
        }

        private void UpdateBoidsInRealTime()
        {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            ManipulateBoidDataJob manipulateBoidDataJob = new ManipulateBoidDataJob
            {
                entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                entities = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BoidTag>()).ToEntityArray(Allocator.TempJob),
                minForwardSpeed = _minSpeed,
                maxForwardSpeed = _maxSpeed,
                rotationSpeed = _rotationSpeed,
                boidBoundsSize = _spawnBounds,
                boidBoundsCenter = _spawnBoundsCenter,
                insideBoundsDistance = _insideBoundsDistance,
                insideBoundsWeight = _insideBoundsWeight,
                cohesionDistance = _cohesionDistance,
                cohesionWeight = _cohesionWeight,
                avoidanceDistance = _avoidanceDistance,
                avoidanceWeight = _avoidanceWeight,
                alignmentDistance = _aligementDistance,
                alignmentWeight = _aligementWeight
            };

            JobHandle manipulationHandle = manipulateBoidDataJob.Schedule(_boidSize, 128);
            manipulationHandle.Complete();

            entityCommandBuffer.Playback(_entityManager);
            entityCommandBuffer.Dispose();
        }
    }
}
