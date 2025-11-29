using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    public class WindTracer : MonoBehaviour
    {
        private static readonly int Launch = Animator.StringToHash("Launch");
        private static readonly int Rng = Animator.StringToHash("rng");

        [SerializeField]
        private Animator _windEffect;

        [SerializeField]
        private Vector3 _scale = Vector3.one;

        [SerializeField]
        private float _effectWidth;
        
        private Camera _camera;
        private bool _tracing;
        private Vector2 _start;
        private Vector2 _end;
        
        private void Awake()
        {
            _camera = Camera.main;
        }

        private void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                _start = GetMousePosition();
            }
            else if (!value.isPressed)
            {
                _end = GetMousePosition();
                
                ProcessWindTrace(_start, _end);
                
                _start = Vector2.zero;
                _end = Vector2.zero;
            }
        }

        private void ProcessWindTrace(Vector2 start, Vector2 end)
        {
            var effect = SpawnWindEffect(start, end);
            CreateLineCollider(effect, start, end);
            Destroy(effect, 1f);
        }

        private Transform SpawnWindEffect(Vector2 start, Vector2 end)
        {
            var direction = (end - start).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var windEffect = Instantiate(_windEffect, start, Quaternion.Euler(0, 0, angle));

            windEffect.transform.localScale = _scale;

            windEffect.SetFloat(Rng, Random.value > 0.5f ? 0 : 1);
            windEffect.SetTrigger(Launch);
            
            return windEffect.transform;
        }

        private void CreateLineCollider(Transform effect, Vector2 start, Vector2 end)
        {
            var obj = new GameObject("LineCollider2D");
            obj.transform.SetParent(effect);
            obj.transform.position = (start + end) / 2f;
            obj.transform.localRotation = Quaternion.identity;

            var direction = end - start;
            var length = direction.magnitude;

            var effectCollider = obj.AddComponent<BoxCollider2D>();
            effectCollider.size = new Vector2(length, _effectWidth);
            effectCollider.offset = Vector2.zero;
        }
        
        private Vector3 GetMousePosition()
        {
            return _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
    }
}