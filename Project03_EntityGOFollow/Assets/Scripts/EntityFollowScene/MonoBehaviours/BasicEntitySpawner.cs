using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace ATM.DOTS.Project03
{
    public class BasicEntitySpawner : MonoBehaviour
    {
        [SerializeField]
        private Mesh _mesh;
        [SerializeField]
        private Material _material;

        private EntityManager _entityManager;

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityArchetype entityArchetype = _entityManager.CreateArchetype(
                // Tags
                typeof(LeaderTagComponent),
                // Built-in
                typeof(Translation),
                // Custom
                typeof(DirectionDataComponent),
                typeof(MovementDataComponent),
                typeof(MaterialColorDataComponent),
                typeof(MaterialSmoothnessDataComponent),
                typeof(MaterialAODataComponent)
            );

            Entity entity = _entityManager.CreateEntity(entityArchetype);
#if UNITY_EDITOR
            _entityManager.SetName(entity, "Entity");
#endif

            RenderMeshUtility.AddComponents(
                entity,
                _entityManager,
                new RenderMeshDescription(mesh: _mesh,
                                          material: _material,
                                          shadowCastingMode: UnityEngine.Rendering.ShadowCastingMode.On,
                                          receiveShadows: true)
            );
            _entityManager.AddComponentData(entity, new Translation
            {
                Value = new float3(0f, 0f, 0f)
            });
            _entityManager.AddComponentData(entity, new MovementDataComponent
            {
                horizontalAcceleration = 7.5f
            });
            _entityManager.AddComponentData(entity, new MaterialColorDataComponent
            {
                materialColor = new float4(0f, 1f, 0f, 1f)
            });
            _entityManager.AddComponentData(entity, new MaterialSmoothnessDataComponent
            {
                smoothness = 0.2f
            });
            _entityManager.AddComponentData(entity, new MaterialAODataComponent
            {
                ambientOcclusion = 0.2f
            });
        }
    }
}