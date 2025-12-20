using System;
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

        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
        private static readonly int ColorID = Shader.PropertyToID("_Color");

        public bool IsDetached { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            // Appliquer la couleur du renderer à l'émission et à l'albedo
            if (_spriteRenderer != null)
            {
                _propertyBlock = new MaterialPropertyBlock();
                _spriteRenderer.GetPropertyBlock(_propertyBlock);

                // Teinter l'émission et l'albedo avec la couleur du sprite
                Color color = _spriteRenderer.color;
                Color emissionColor = color * 4f; // Intensité de 4 pour l'émission
                Color albedoColor = new Color(color.r, color.g, color.b, 0.5f); // 50% d'opacité pour l'albedo

                _propertyBlock.SetColor(EmissionColorID, emissionColor);
                _propertyBlock.SetColor(ColorID, albedoColor);

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