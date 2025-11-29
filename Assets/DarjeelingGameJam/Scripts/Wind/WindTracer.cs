using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    public class WindTracer : MonoBehaviour
    {
        [SerializeField]
        private WindGust _windEffect;
        
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
            Destroy(effect.gameObject, 1f);
        }

        private Transform SpawnWindEffect(Vector2 start, Vector2 end)
        {
            var direction = end - start;
            var angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg;
            var windEffect = Instantiate(_windEffect, start, Quaternion.Euler(0, 0, angle));
            windEffect.Initialize(direction);
            return windEffect.transform;
        }

        private Vector3 GetMousePosition()
        {
            return _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
    }
}