using Modules.Toolbag.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    public class WindTracer : MonoBehaviour
    {
        [SerializeField]
        private WindGust _windEffect;
        
        private Camera _camera;
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
                _start = Mouse.current.position.ReadValue();
            }
            else if (!value.isPressed)
            {
                _end = Mouse.current.position.ReadValue();

                if (!_start.AlmostEqual(_end, 1f))
                {
                    var worldStart = _camera.ScreenToWorldPoint(_start);
                    var worldEnd = _camera.ScreenToWorldPoint(_end);
                    ProcessWindTrace(worldStart, worldEnd);
                }
                
                _start = Vector2.zero;
                _end = Vector2.zero;
            }
        }

        private void ProcessWindTrace(Vector2 start, Vector2 end)
        {
            SpawnWindEffect(start, end);
        }

        private Transform SpawnWindEffect(Vector2 start, Vector2 end)
        {
            var direction = end - start;
            var angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg;
            var windEffect = Instantiate(_windEffect, start, Quaternion.Euler(0, 0, angle));
            windEffect.Initialize(direction);
            return windEffect.transform;
        }
    }
}