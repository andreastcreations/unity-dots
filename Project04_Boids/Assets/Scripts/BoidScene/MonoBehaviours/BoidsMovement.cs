using UnityEngine;

namespace ATM.DOTS.Project04
{
    /// <summary>
    /// A required component that sets all the movement data of a group of boids.
    /// </summary>
    public class BoidsMovement : MonoBehaviour
    {
        [Range(0f, 10f)]
        [SerializeField] private float _minSpeed;
        [Range(0f, 10f)]
        [SerializeField] private float _maxSpeed;
        [Range(0f, 10f)]
        [SerializeField] private float _rotationSpeed;

        public float MinSpeed => _minSpeed;
        public float MaxSpeed => _maxSpeed;
        public float RotationSpeed => _rotationSpeed;
    }
}
