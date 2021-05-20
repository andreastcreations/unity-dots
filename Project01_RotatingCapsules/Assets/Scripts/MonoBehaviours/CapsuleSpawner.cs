using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;

using Random = Unity.Mathematics.Random;

namespace ATM.DOTS.Project01
{
    public class CapsuleSpawner : MonoBehaviour
    {
        [SerializeField]
        private int _spawnCount = 10;
        [SerializeField]
        private Mesh _mesh;
        [SerializeField]
        private Material _material;

        private EntityManager _entityManager;
        private Random _random;

        [BurstCompatible]
        public struct CapsuleSpawnJob : IJobParallelFor
        {
            public Entity entityPrototype;
            public int entityCount;
            public Random entitySeed;
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

            public void Execute(int index)
            {
                Entity entity = entityCommandBuffer.Instantiate(index, entityPrototype);

                entityCommandBuffer.SetComponent(index, entity, new Translation
                {
                    Value = new float3(entitySeed.NextFloat(-25f, 25f),
                                       entitySeed.NextFloat(-15f, 15f),
                                       entitySeed.NextFloat(-25f, 25f))
                });
                entityCommandBuffer.SetComponent(index, entity, new Rotation
                {
                    Value = quaternion.RotateY(entitySeed.NextFloat(-10f, 10f))
                });
                entityCommandBuffer.SetComponent(index, entity, new Scale
                {
                    Value = 0.25f
                });
                entityCommandBuffer.SetComponent(index, entity, new RotationDataComponent
                {
                    rotationSpeed = entitySeed.NextFloat(30f, 70f)
                });
                entityCommandBuffer.SetComponent(index, entity, new MaterialColorDataComponent
                {
                    materialColor = new float4(entitySeed.NextFloat(0f, 1f),
                                               entitySeed.NextFloat(0f, 1f),
                                               entitySeed.NextFloat(0f, 1f),
                                               1f)
                });
                entityCommandBuffer.SetComponent(index, entity, new MaterialEmissionDataComponent
                {
                    emission = new float3(entitySeed.NextFloat(0f, 1f),
                                          entitySeed.NextFloat(0f, 1f),
                                          entitySeed.NextFloat(0f, 1f))
                });
            }
        }

        private void Awake()
        {
            _random = new Random(100);
        }

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            EntityArchetype capsuleArchetype = _entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(LocalToWorld),

                typeof(RotationDataComponent),
                typeof(MaterialColorDataComponent),
                typeof(MaterialEmissionDataComponent)
            );

            Entity capsulePrototype = _entityManager.CreateEntity(capsuleArchetype);
#if UNITY_EDITOR
            _entityManager.SetName(capsulePrototype, "Capsule");
#endif

            RenderMeshDescription description = new RenderMeshDescription(_mesh, _material);
            RenderMeshUtility.AddComponents(capsulePrototype, _entityManager, description);

            CapsuleSpawnJob capsuleSpawnJob = new CapsuleSpawnJob
            {
                entityPrototype = capsulePrototype,
                entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                entityCount = _spawnCount,
                entitySeed = _random
            };

            JobHandle spawnHandle = capsuleSpawnJob.Schedule(_spawnCount, 128);
            spawnHandle.Complete();

            entityCommandBuffer.Playback(_entityManager);
            entityCommandBuffer.Dispose();
            _entityManager.DestroyEntity(capsulePrototype);
        }
    }
}