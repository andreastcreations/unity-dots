using UnityEngine;

namespace ATM.DOTS.Project04
{
    /// <summary>
    /// A required component that sets the bounding box of a group of boids.
    /// </summary>
    public class BoidsBounds : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _size;
        [SerializeField]
        private Vector3 _center;
        [SerializeField]
        private int _partitionsSize;

        public Vector3 Size => _size;
        public Vector3 Center => _center;
        public int PartitionsSize => _partitionsSize;
    }
}
