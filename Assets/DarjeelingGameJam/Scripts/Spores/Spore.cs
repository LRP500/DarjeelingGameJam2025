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

        [Tooltip("Profondeur de pénétration dans le ground (0 = surface, 1 = fond du ground)")]
        [SerializeField]
        private Vector2 _germinationDepthRange = new Vector2(0.1f, 0.9f);

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
        private float _germinationDepthRatio; // Ratio de pénétration (0-1)
        private float _germinationYThreshold; // Y absolu calculé depuis le ground
        private bool _germinationYCalculated = false;
        private bool _canGerminate = false;

        public bool IsDetached { get; private set; }

        public void SetCanSpawnPlant(bool canSpawnPlant)
        {
            _canSpawnPlant = canSpawnPlant;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            // Optimisation: Discrete collision detection (moins précis mais beaucoup plus rapide)
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

            // Appliquer une gravité aléatoire dans la plage définie
            _rigidbody.gravityScale = UnityEngine.Random.Range(_minGravityScale, _maxGravityScale);

            // Déterminer le ratio de pénétration aléatoire pour cette spore
            _germinationDepthRatio = UnityEngine.Random.Range(_germinationDepthRange.x, _germinationDepthRange.y);

            // Sauvegarder les valeurs cibles avant de commencer l'animation
            if (_spriteRenderer != null)
            {
                _propertyBlock = new MaterialPropertyBlock();
                _spriteRenderer.GetPropertyBlock(_propertyBlock);

                // Calculer les couleurs cibles
                _targetColor = _spriteRenderer.color;
                _targetEmissionColor = _targetColor * 4f; // Intensité de 4 pour l'émission
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

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Ground"))
            {
                // Calculer le seuil Y la première fois qu'on entre dans le ground
                if (!_germinationYCalculated)
                {
                    // Récupérer les bounds du ground collider
                    Bounds groundBounds = other.bounds;
                    float groundTop = groundBounds.max.y;    // Haut du ground
                    float groundBottom = groundBounds.min.y; // Bas du ground

                    // Calculer le Y absolu basé sur le ratio de pénétration
                    // 0 = groundTop (surface), 1 = groundBottom (fond)
                    _germinationYThreshold = Mathf.Lerp(groundTop, groundBottom, _germinationDepthRatio);
                    _germinationYCalculated = true;
                }

                // Vérifier d'abord si le délai est écoulé
                if (!_canGerminate)
                    return;

                // Vérifier si la spore est assez basse pour germer
                if (transform.position.y <= _germinationYThreshold)
                {
                    Germinate(transform.position);
                }
                // Sinon, ne rien faire - la spore continue de tomber jusqu'à être assez basse
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