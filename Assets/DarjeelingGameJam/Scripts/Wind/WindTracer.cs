using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    [RequireComponent(typeof(LineRenderer))]
    public class WindTracer : MonoBehaviour
    {
        private Camera _camera;
        private bool _tracing;
        private Vector2 _start;
        private Vector2 _end;
        
        private LineRenderer _line;

        private void Awake()
        {
            _camera = Camera.main;
            _line = GetComponent<LineRenderer>();
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
            _line.positionCount = 2;
            _line.SetPosition(0, start);
            _line.SetPosition(1, end);
        }

        private Vector3 GetMousePosition()
        {
            return _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
    }
}