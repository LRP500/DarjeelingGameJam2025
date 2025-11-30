using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DarjeelingGameJam.Spores;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

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
        
        [MinValue(0)]
        [SerializeField]
        private float _plantEffectDuration = 2f;
        
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
                HandleSporeCollision(other);
            }
            else if (other.CompareTag("Plant"))
            {
                HandlePlantCollision(other).Forget();
            }
        }
        
        private void HandleSporeCollision(Collider2D other)
        {
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
        
        private async UniTask HandlePlantCollision(Collider2D other)
        {
            if (other.TryGetComponent<LoopEndOfClip>(out var wind))
            {
                wind.windActive = true;
                
                await Task.Delay(
                    TimeSpan.FromSeconds(_plantEffectDuration));
                
                wind.windActive = false;
            }
        }
    }
}