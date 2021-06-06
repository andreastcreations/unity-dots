using UnityEngine;

namespace ATM.DOTS.Project04
{
    /// <summary>
    /// A required component that sets all the behaviours' data of a group of boids.
    /// </summary>
    public class BoidsBehaviours : MonoBehaviour
    {
        [SerializeField]
        private BoidBehaviour _cohesion;
        [SerializeField]
        private BoidBehaviour _avoidance;
        [SerializeField]
        private BoidBehaviour _alignment;
        [SerializeField]
        private BoidBehaviour _insideBounds;

        public BoidBehaviour Cohesion => _cohesion;
        public BoidBehaviour Avoidance => _avoidance;
        public BoidBehaviour Alignment => _alignment;
        public BoidBehaviour InsideBounds => _insideBounds;

    }

    /// <summary>
    /// Basic information about each behaviour.
    /// </summary>
    [System.Serializable]
    public struct BoidBehaviour
    {
        [Range(0f, 10f)]
        public float distance;
        [Range(0f, 10f)]
        public float weight;
    }
}
