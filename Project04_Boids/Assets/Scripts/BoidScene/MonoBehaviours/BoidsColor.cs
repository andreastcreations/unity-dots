using UnityEngine;

namespace ATM.DOTS.Project04
{
    /// <summary>
    /// A required component that sets all the color data of a group of boids.
    /// </summary>
    public class BoidsColor : MonoBehaviour
    {
        [SerializeField]
        private Color _color;
        [SerializeField]
        private Color _emission;

        public Color Color => _color;
        public Color Emission => _emission;
    }
}
