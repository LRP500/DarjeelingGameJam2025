using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// Tracks mouse velocity with smoothing to reduce jitter.
    /// Provides screen-space and world-space velocity data.
    /// </summary>
    public class MouseVelocityTracker : MonoBehaviour
    {
        [Header("Smoothing Settings")]
        [Tooltip("Number of frames to average for velocity smoothing")]
        [SerializeField]
        private int _velocitySmoothFrames = 5;

        [Header("Thresholds")]
        [Tooltip("Minimum speed in world units/second to register as movement")]
        [SerializeField]
        private float _minSpeedThreshold = 0.5f;

        [Tooltip("Maximum speed in world units/second for normalization")]
        [SerializeField]
        private float _maxSpeedClamp = 30f;

        private Camera _camera;
        private Vector2 _previousMousePosition;
        private Queue<Vector2> _velocityHistory;

        /// <summary>
        /// Current smoothed velocity in screen space (pixels/second)
        /// </summary>
        public Vector2 CurrentVelocityScreen { get; private set; }

        /// <summary>
        /// Current smoothed velocity in world space
        /// </summary>
        public Vector2 CurrentVelocityWorld { get; private set; }

        /// <summary>
        /// Current speed magnitude in world units/second
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Normalized speed (0-1) based on maxSpeedClamp
        /// </summary>
        public float NormalizedSpeed { get; private set; }

        /// <summary>
        /// Whether the mouse is moving above the minimum threshold
        /// </summary>
        public bool IsMoving => Speed >= _minSpeedThreshold;

        private void Awake()
        {
            _camera = Camera.main;
            _velocityHistory = new Queue<Vector2>(_velocitySmoothFrames);
            _previousMousePosition = Mouse.current.position.ReadValue();
        }

        private void Update()
        {
            CalculateVelocity();
        }

        private void CalculateVelocity()
        {
            // Get current mouse position
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();

            // Calculate instantaneous velocity in screen space
            Vector2 instantVelocity = (currentMousePosition - _previousMousePosition) / Time.deltaTime;

            // Add to history for smoothing
            _velocityHistory.Enqueue(instantVelocity);
            if (_velocityHistory.Count > _velocitySmoothFrames)
            {
                _velocityHistory.Dequeue();
            }

            // Calculate smoothed velocity (average)
            Vector2 smoothedVelocity = Vector2.zero;
            foreach (var velocity in _velocityHistory)
            {
                smoothedVelocity += velocity;
            }
            smoothedVelocity /= _velocityHistory.Count;

            // Update screen velocity
            CurrentVelocityScreen = smoothedVelocity;

            // Convert to world space
            Vector2 screenPoint1 = _previousMousePosition;
            Vector2 screenPoint2 = _previousMousePosition + smoothedVelocity * Time.deltaTime;

            Vector3 worldPoint1 = _camera.ScreenToWorldPoint(new Vector3(screenPoint1.x, screenPoint1.y, _camera.nearClipPlane));
            Vector3 worldPoint2 = _camera.ScreenToWorldPoint(new Vector3(screenPoint2.x, screenPoint2.y, _camera.nearClipPlane));

            CurrentVelocityWorld = ((Vector2)(worldPoint2 - worldPoint1)) / Time.deltaTime;

            // Calculate speed and normalization based on WORLD space (indépendant de la résolution)
            Speed = CurrentVelocityWorld.magnitude;
            NormalizedSpeed = Mathf.Clamp01(Speed / _maxSpeedClamp);

            // Update previous position
            _previousMousePosition = currentMousePosition;
        }

        /// <summary>
        /// Get the normalized direction of movement in world space
        /// </summary>
        public Vector2 GetDirection()
        {
            return CurrentVelocityWorld.normalized;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _camera == null)
                return;

            // Draw velocity vector for debugging
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _camera.nearClipPlane));

            Gizmos.color = IsMoving ? Color.green : Color.red;
            Gizmos.DrawWireSphere(worldPos, 0.2f);

            if (IsMoving)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(worldPos, CurrentVelocityWorld.normalized * 2f);
            }
        }
    }
}
