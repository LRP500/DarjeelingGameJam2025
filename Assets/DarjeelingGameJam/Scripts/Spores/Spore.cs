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

        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private Color _targetColor;
        private Color _targetEmissionColor;
        private Color _targetAlbedoColor;
        private Vector3 _targetScale;

        public bool IsDetached { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

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
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                var closestPoint = other.collider.ClosestPoint(transform.position);
                Germinate(closestPoint);
            }
        }

        private void Germinate(Vector2 closestPoint)
        {
            var position = new Vector3(closestPoint.x, closestPoint.y);
            Instantiate(_plant, position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}