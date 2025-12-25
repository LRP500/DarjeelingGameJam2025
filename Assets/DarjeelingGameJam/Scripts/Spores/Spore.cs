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

        [Header("Material Variants (pour GPU Instancing)")]
        [Tooltip("Matériaux pré-configurés avec différentes couleurs/émissions. Un sera choisi aléatoirement.")]
        [SerializeField]
        private Material[] _materialVariants;

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
        private Color _targetColor;
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

            // Choisir un matériau aléatoire pour la variété (permet le GPU Instancing)
            if (_spriteRenderer != null && _materialVariants != null && _materialVariants.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, _materialVariants.Length);
                _spriteRenderer.material = _materialVariants[randomIndex];

                // Sauvegarder la couleur cible et commencer transparent pour l'animation
                _targetColor = _spriteRenderer.color;
                _spriteRenderer.color = new Color(_targetColor.r, _targetColor.g, _targetColor.b, 0f);
            }
            else if (_spriteRenderer != null)
            {
                // Fallback: utiliser le matériau par défaut
                _targetColor = _spriteRenderer.color;
                _spriteRenderer.color = new Color(_targetColor.r, _targetColor.g, _targetColor.b, 0f);
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

                // Animer le fade sur le sprite renderer
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = new Color(
                        _targetColor.r,
                        _targetColor.g,
                        _targetColor.b,
                        _targetColor.a * curveValue
                    );
                }

                yield return null;
            }

            // S'assurer que les valeurs finales sont exactes
            transform.localScale = _targetScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _targetColor;
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