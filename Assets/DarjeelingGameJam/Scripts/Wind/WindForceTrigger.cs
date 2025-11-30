using DarjeelingGameJam.Spores;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DarjeelingGameJam.Wind
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class WindForceTrigger : MonoBehaviour
    {
        [MinValue(0.1)]
        [SerializeField]
        private float _windForce = 1f;

        [Range(0, 1)]
        [SerializeField]
        private float _sporeDetachChance = 0.5f;
        
        private Vector3 _direction;
        
        public BoxCollider2D Collider { get; private set; }

        private void Awake()
        {
            Collider = GetComponent<BoxCollider2D>();
        }

        public void Initialize(Vector3 direction)
        {
            _direction = direction.normalized;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Spore"))
            {
                return;
            }
            
            var spore = other.GetComponent<Spore>();

            if (!spore.IsDetached && Random.value < _sporeDetachChance)
            {
                spore.Detach();
            }

            if (spore.IsDetached)
            {
                other.attachedRigidbody.AddForce(_direction * _windForce, ForceMode2D.Impulse);
            }
        }
    }
}