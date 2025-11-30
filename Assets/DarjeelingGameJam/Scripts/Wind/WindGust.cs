using R3;
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
        private WindForceTrigger _forceTrigger;
        
        [SerializeField]
        private Vector3 _scale = Vector3.one;

        [MinValue(1)]
        [SerializeField]
        private float _effectWidth = 4f;

        public void Initialize(Vector3 direction)
        {
            _animator.transform.localScale = _scale;

            var position = _forceTrigger.transform.localPosition;
            position.x += direction.magnitude / 2;
            position.y = 0f;
            
            _forceTrigger.Initialize(direction);
            _forceTrigger.transform.localPosition = position;
            _forceTrigger.Collider.size = new Vector2(direction.magnitude, _effectWidth);
            _forceTrigger.Collider.offset = Vector2.zero;
            
            _animator.SetFloat(Rng, Random.value > 0.5f ? 0 : 1);
            _animator.SetTrigger(Launch);

            Invoke(nameof(Hide), 1f);
            Destroy(gameObject, 4f);
        }

        private void Hide()
        {
            _forceTrigger.gameObject.SetActive(false);
            _animator.gameObject.SetActive(false);
        }
    }
}