using System;
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
            if (other.CompareTag("Spore"))
            {
                other.attachedRigidbody.AddForce(_direction * _windForce, ForceMode2D.Impulse);
            }
        }
    }
}