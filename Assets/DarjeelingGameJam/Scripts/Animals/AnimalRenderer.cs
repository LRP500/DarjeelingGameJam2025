using UnityEngine;

namespace DarjeelingGameJam.Animals
{
    public class AnimalRenderer : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _rigidbody = GetComponentInParent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _spriteRenderer.flipX = _rigidbody.linearVelocity.x > 0;
            // _spriteRenderer.flipY = _rigidbody.linearVelocity.y < 0;
        }
    }
}