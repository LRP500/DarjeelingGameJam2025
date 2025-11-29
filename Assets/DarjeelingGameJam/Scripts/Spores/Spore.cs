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
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        [Button]
        private void Attach()
        {
            _rigidbody.simulated = false;
        }
        
        [Button]
        public void Detach()
        {
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