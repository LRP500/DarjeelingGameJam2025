using UnityEngine;

namespace DarjeelingGameJam.Parallax
{
    public class ParallaxLayer : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField]
        private float _verticalMultiplier;

        [Range(0, 1)]
        [SerializeField]
        private float _horizontalMultiplier;
        
        public void Move(Vector3 delta)
        {
            var movement = new Vector3(
                delta.x * _horizontalMultiplier,
                delta.y * _verticalMultiplier,
                0f);

            transform.position += movement;
        }
    }
}