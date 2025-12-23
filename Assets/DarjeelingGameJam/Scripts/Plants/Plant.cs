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

        [Tooltip("Si coché, flip Y au lieu de flip X")]
        [SerializeField]
        private bool _useFlipY = false;

        private void Awake()
        {
            var scale = Random.Range(_scaleMinMax.x, _scaleMinMax.y);
            transform.localScale = new Vector3(scale, scale, 0);

            // Flip aléatoire (X ou Y selon le paramètre)
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (_useFlipY)
                {
                    spriteRenderer.flipY = Random.value > 0.5f;
                }
                else
                {
                    spriteRenderer.flipX = Random.value > 0.5f;
                }
            }

            // Rotation aléatoire en Y (très légère)
            float randomRotationY = Random.Range(-_maxRotationY, _maxRotationY);
            transform.rotation = Quaternion.Euler(0, randomRotationY, 0);

            // Assigner des seeds aléatoires pour le vent et la variation de teinte dans le shader
            var plantMaterialProperties = GetComponent<PlantMaterialProperties>();
            if (plantMaterialProperties != null)
            {
                plantMaterialProperties.SetRandomSeed(Random.Range(0f, 1000f));  // Pour le vent
                plantMaterialProperties.SetTintSeed(Random.Range(0f, 1000f));    // Pour la teinte
            }
        }
    }
}