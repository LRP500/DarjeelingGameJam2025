using Unity.Cinemachine;
using UnityEngine;

namespace DarjeelingGameJam.Plants
{
    public class Plant : MonoBehaviour
    {
        [Vector2AsRange]
        [SerializeField]
        private Vector2 _scaleMinMax = new(0.7f, 1.3f);

        [SerializeField]
        private float _maxRotationY = 10f;

        private void Awake()
        {
            var scale = Random.Range(_scaleMinMax.x, _scaleMinMax.y);
            transform.localScale = new Vector3(scale, scale, 0);

            // Flip horizontal aléatoire pour plus de variation
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = Random.value > 0.5f;
            }

            // Rotation aléatoire en Y (très légère)
            float randomRotationY = Random.Range(-_maxRotationY, _maxRotationY);
            transform.rotation = Quaternion.Euler(0, randomRotationY, 0);
        }
    }
}