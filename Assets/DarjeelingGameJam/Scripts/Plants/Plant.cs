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

        [Header("Variations Exceptionnelles (rares)")]
        [Range(0f, 1f)]
        [Tooltip("Probabilité d'avoir une plante exceptionnelle (0 = jamais, 0.1 = 10%)")]
        [SerializeField]
        private float _exceptionalChance = 0.1f;

        [Vector2AsRange]
        [Tooltip("Range de taille pour les plantes exceptionnelles")]
        [SerializeField]
        private Vector2 _exceptionalScaleMinMax = new(0.4f, 2.5f);

        [Range(0f, 1.5f)]
        [Tooltip("Variation de teinte pour les plantes exceptionnelles (1.0 = couleurs folles)")]
        [SerializeField]
        private float _exceptionalTintVariation = 1.0f;

        private void Awake()
        {
            // Tirer au sort : plante normale ou exceptionnelle ?
            bool isExceptional = Random.value < _exceptionalChance;

            // Choisir la taille : normale (tes valeurs) ou exceptionnelle
            Vector2 scaleRange = isExceptional ? _exceptionalScaleMinMax : _scaleMinMax;
            float scale = Random.Range(scaleRange.x, scaleRange.y);
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

                // Si exceptionnelle, appliquer une variation de teinte plus intense
                if (isExceptional)
                {
                    plantMaterialProperties.SetTintVariationAmount(_exceptionalTintVariation);
                }
            }
        }
    }
}