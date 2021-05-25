using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ATM.DOTS.Project03
{
    public class EntityFollower : MonoBehaviour
    {
        [SerializeField]
        private float _followSpeed;
        [SerializeField]
        private float _followDistance;

        private EntityManager _entityManager;
        private EntityQuery _entityQuery;
        private NativeArray<Entity> _entities;

        Vector3 entityPosition = new Vector3();

        private void OnDisable() => _entities.Dispose();

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _entityQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<LeaderTagComponent>(),
                                                            ComponentType.ReadOnly<Translation>());
        }

        private void Update()
        {
            if (!TryGetEntitiesArray()) return;

            float3 ePosValue = _entityManager.GetComponentData<Translation>(_entities[0]).Value; //There is only one entity. Hard-coded.
            entityPosition = new Vector3(ePosValue.x, ePosValue.y, ePosValue.z);

            if (transform.position.sqrMagnitude + entityPosition.sqrMagnitude != _followDistance * _followDistance)
            {
                Vector3 newPosition = entityPosition + (transform.position - entityPosition).normalized * _followDistance;
                transform.position = Vector3.Slerp(transform.position, newPosition, _followSpeed * Time.deltaTime);
            }

            transform.LookAt(entityPosition);
        }

        private bool TryGetEntitiesArray()
        {
            if (_entities == null || _entities.Length <= 0f)
            {
                _entities = _entityQuery.ToEntityArray(Allocator.Persistent);
            }

            if (_entities == null)
            {
                return false;
            }
            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            GUIStyle style = new GUIStyle
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };

            UnityEditor.Handles.Label(transform.position + new Vector3(0f, 2f, 0f),
                                      "GameObject",
                                      style);
            if (Application.isPlaying)
            {
                UnityEditor.Handles.Label(entityPosition + new Vector3(0f, 2f, 0f),
                                          "Entity",
                                          style);
            }
        }
#endif
    }
}
