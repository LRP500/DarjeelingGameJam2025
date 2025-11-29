using Sirenix.OdinInspector;
using UnityEngine;

namespace DarjeelingGameJam.Wind
{
    public class WindGust : MonoBehaviour
    {
        private static readonly int Launch = Animator.StringToHash("Launch");
        private static readonly int Rng = Animator.StringToHash("rng");
        
        [SerializeField]
        private Animator _animator;
        
        [SerializeField]
        private BoxCollider2D _collider;
        
        [SerializeField]
        private Vector3 _scale = Vector3.one;

        [MinValue(1)]
        [SerializeField]
        private float _effectWidth = 4f;

        [MinValue(0.1)]
        [SerializeField]
        private float _windForce = 1f;
        
        public void Initialize(Vector3 direction)
        {
            _animator.transform.localScale = _scale;

            var position = _collider.transform.localPosition;
            position.x += direction.magnitude / 2;
            position.y = 0f;
            
            _collider.transform.localPosition = position;
            _collider.size = new Vector2(direction.magnitude, _effectWidth);
            _collider.offset = Vector2.zero;
            
            _animator.SetFloat(Rng, Random.value > 0.5f ? 0 : 1);
            _animator.SetTrigger(Launch);
        }
    }
}