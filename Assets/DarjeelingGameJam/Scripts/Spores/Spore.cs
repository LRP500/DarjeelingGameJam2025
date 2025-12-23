using System;
using System.Collections;
using DarjeelingGameJam.Plants;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DarjeelingGameJam.Spores
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Spore : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private Plant _plant;

        [Header("Spawn Animation")]
        [Tooltip("Durée de l'animation d'apparition")]
        [MinValue(0f)]
        [SerializeField]
        private float _spawnDuration = 0.3f;

        [Tooltip("Courbe d'animation pour le scale et le fade")]
        [SerializeField]
        private AnimationCurve _spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("Intensité de l'émission lumineuse (4 = défaut, 8+ = très lumineux)")]
        [MinValue(0f)]
        [SerializeField]
        private float _emissionIntensity = 4f;

        [Tooltip("Réduction de la saturation des couleurs (0 = gris, 1 = couleur normale)")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _colorSaturation = 0.6f;

        [Header("Physics Settings")]
        [Tooltip("Gravité minimale de la spore")]
        [MinValue(0f)]
        [SerializeField]
        private float _minGravityScale = 0.05f;

        [Tooltip("Gravité maximale de la spore")]
        [MinValue(0f)]
        [SerializeField]
        private float _maxGravityScale = 0.15f;

        [Header("Germination Settings")]
        [Tooltip("Est-ce que cette spore peut faire germer une plante ?")]
        [SerializeField]
        private bool _canSpawnPlant = true;

        [Tooltip("Hauteur Y maximale de germination (plus haut)")]
        [SerializeField]
        private float _germinationMaxY = 0f;

        [Tooltip("Hauteur Y minimale de germination (plus bas)")]
        [SerializeField]
        private float _germinationMinY = -5f;

        [Tooltip("Délai (en secondes) après détachement avant de pouvoir germer")]
        [MinValue(0f)]
        [SerializeField]
        private float _germinationDelay = 1f;

        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private Color _targetColor;
        private Color _targetEmissionColor;
        private Color _targetAlbedoColor;
        private Vector3 _targetScale;
        private float _germinationYThreshold; // Y absolu de germination (tiré aléatoirement)
        private bool _canGerminate = false;
        private float _germinationCheckTimer = 0f;
        private const float GerminationCheckInterval = 0.2f; // Check toutes les 0.2 secondes

        public bool IsDetached { get; private set; }

        public void SetCanSpawnPlant(bool canSpawnPlant)
        {
            _canSpawnPlant = canSpawnPlant;
        }

        private Color DesaturateColor(Color color, float saturation)
        {
            // Convertir en grayscale
            float gray = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;

            // Interpoler entre grayscale et couleur originale
            return new Color(
                Mathf.Lerp(gray, color.r, saturation),
                Mathf.Lerp(gray, color.g, saturation),
                Mathf.Lerp(gray, color.b, saturation),
                color.a
            );
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            // Optimisation: Discrete collision detection (moins précis mais beaucoup plus rapide)
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

            // Appliquer une gravité aléatoire dans la plage définie
            _rigidbody.gravityScale = UnityEngine.Random.Range(_minGravityScale, _maxGravityScale);

            // Tirer aléatoirement la hauteur Y de germination entre min et max
            _germinationYThreshold = UnityEngine.Random.Range(_germinationMinY, _germinationMaxY);

            // Sauvegarder les valeurs cibles avant de commencer l'animation
            if (_spriteRenderer != null)
            {
                _propertyBlock = new MaterialPropertyBlock();
                _spriteRenderer.GetPropertyBlock(_propertyBlock);

                // Calculer les couleurs cibles avec désaturation
                _targetColor = DesaturateColor(_spriteRenderer.color, _colorSaturation);
                _targetEmissionColor = _targetColor * _emissionIntensity;
                _targetAlbedoColor = new Color(_targetColor.r, _targetColor.g, _targetColor.b, 0.5f); // 50% d'opacité pour l'albedo

                // Commencer avec des valeurs à zéro pour l'animation
                _spriteRenderer.color = new Color(_targetColor.r, _targetColor.g, _targetColor.b, 0f);
                _propertyBlock.SetColor(EmissionColorID, Color.black);
                _propertyBlock.SetColor(ColorID, new Color(_targetAlbedoColor.r, _targetAlbedoColor.g, _targetAlbedoColor.b, 0f));
                _spriteRenderer.SetPropertyBlock(_propertyBlock);
            }

            // Sauvegarder le scale cible et commencer à 0
            _targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        private void Start()
        {
            // Lancer l'animation de spawn
            StartCoroutine(SpawnAnimation());
        }

        private IEnumerator SpawnAnimation()
        {
            float elapsed = 0f;

            while (elapsed < _spawnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _spawnDuration;
                float curveValue = _spawnCurve.Evaluate(t);

                // Animer le scale
                transform.localScale = _targetScale * curveValue;

                // Animer le fade sur le sprite renderer et les couleurs du material
                if (_spriteRenderer != null)
                {
                    // Fade de la couleur du sprite
                    _spriteRenderer.color = new Color(
                        _targetColor.r,
                        _targetColor.g,
                        _targetColor.b,
                        _targetColor.a * curveValue
                    );

                    // Fade de l'émission et de l'albedo
                    _propertyBlock.SetColor(EmissionColorID, _targetEmissionColor * curveValue);
                    _propertyBlock.SetColor(ColorID, new Color(
                        _targetAlbedoColor.r,
                        _targetAlbedoColor.g,
                        _targetAlbedoColor.b,
                        _targetAlbedoColor.a * curveValue
                    ));
                    _spriteRenderer.SetPropertyBlock(_propertyBlock);
                }

                yield return null;
            }

            // S'assurer que les valeurs finales sont exactes
            transform.localScale = _targetScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _targetColor;
                _propertyBlock.SetColor(EmissionColorID, _targetEmissionColor);
                _propertyBlock.SetColor(ColorID, _targetAlbedoColor);
                _spriteRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        [Button]
        private void Attach()
        {
            IsDetached = false;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
        
        [Button]
        public void Detach()
        {
            IsDetached = true;
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody.simulated = true;

            // Lancer le délai avant de pouvoir germer
            StartCoroutine(EnableGerminationAfterDelay());
        }

        private IEnumerator EnableGerminationAfterDelay()
        {
            _canGerminate = false;
            yield return new WaitForSeconds(_germinationDelay);
            _canGerminate = true;
        }

        private void Update()
        {
            // Ne vérifier la germination que toutes les 0.2 secondes (pas besoin de précision)
            _germinationCheckTimer += Time.deltaTime;
            if (_germinationCheckTimer >= GerminationCheckInterval)
            {
                _germinationCheckTimer = 0f;

                // Vérifier si la spore a atteint la hauteur de germination
                if (_canGerminate && transform.position.y <= _germinationYThreshold)
                {
                    Germinate(transform.position);
                }
            }
        }

        private void Germinate(Vector2 position)
        {
            // Faire apparaître la plante seulement si la spore peut germer
            if (_canSpawnPlant && _plant != null)
            {
                Instantiate(_plant, position, Quaternion.identity);
            }

            // La spore se détruit dans tous les cas (avec ou sans germination)
            Destroy(gameObject);
        }
    }
}