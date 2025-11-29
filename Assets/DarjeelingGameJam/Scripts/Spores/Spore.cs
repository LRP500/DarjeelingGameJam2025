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
        private void Detach()
        {
            _rigidbody.simulated = true;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            Germinate();
        }

        private void Germinate()
        {
            Instantiate(_plant, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}