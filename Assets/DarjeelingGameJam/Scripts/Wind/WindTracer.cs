using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// Generates wind effects continuously based on mouse velocity.
    /// Manages trail rendering and physics trigger spawning.
    /// </summary>
    public class WindTracer : MonoBehaviour
    {
        [Header("References")]
        [Required]
        [SerializeField]
        private MouseVelocityTracker _velocityTracker;

        [Required]
        [SerializeField]
        private WindTriggerSpawner _triggerSpawner;

        [Required]
        [SerializeField]
        private WindParticleController _particleController;

        [Header("Spawn Settings")]
        [Tooltip("Distance in world units between wind particle spawns")]
        [MinValue(0.1f)]
        [SerializeField]
        private float _spawnInterval = 0.5f;

        [Header("Force Mapping")]
        [Tooltip("Minimum wind force when moving slowly")]
        [MinValue(0.001f)]
        [SerializeField]
        private float _minForce = 0.003f;

        [Tooltip("Maximum wind force when moving fast")]
        [MinValue(0.001f)]
        [SerializeField]
        private float _maxForce = 0.03f;

        [Tooltip("Curve to map velocity (0-1) to force multiplier. Use exponential curve for more responsive feel.")]
        [SerializeField]
        private AnimationCurve _velocityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Debug")]
        [Tooltip("Show current force value in scene view")]
        [SerializeField]
        private bool _showDebugInfo = false;

        private Camera _camera;
        private Vector3 _lastSpawnPosition;
        private bool _hasSpawned = false;
        private float _lastForceApplied = 0f;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_velocityTracker == null || _triggerSpawner == null || _particleController == null)
                return;

            // Get current mouse position in world space
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, _camera.nearClipPlane));
            mouseWorldPos.z = 0; // Ensure 2D

            // Get velocity data
            Vector2 direction = _velocityTracker.GetDirection();
            float velocityNormalized = _velocityTracker.NormalizedSpeed;
            bool isMoving = _velocityTracker.IsMoving;

            // Update particle visibility and position
            _particleController.UpdateWindEmission(mouseWorldPos, direction, velocityNormalized, isMoving);

            // Only spawn triggers when moving and distance threshold is met
            if (isMoving && (!_hasSpawned || Vector3.Distance(mouseWorldPos, _lastSpawnPosition) >= _spawnInterval))
            {
                SpawnWindTrigger(mouseWorldPos, direction, velocityNormalized);
                _lastSpawnPosition = mouseWorldPos;
                _hasSpawned = true;
            }
        }

        private void SpawnWindTrigger(Vector3 position, Vector2 direction, float velocityNormalized)
        {
            // Calculate force multiplier based on velocity
            float forceCurveValue = _velocityCurve.Evaluate(velocityNormalized);
            float forceMultiplier = Mathf.Lerp(_minForce, _maxForce, forceCurveValue);

            // Store for debug display
            _lastForceApplied = forceMultiplier;

            // Calculate trigger properties based on velocity
            float triggerLifetime = Mathf.Lerp(0.5f, 1.0f, velocityNormalized);
            float velocityScale = Mathf.Lerp(0.5f, 1.5f, velocityNormalized);

            // Spawn physics trigger
            _triggerSpawner.SpawnWindTrigger(
                position,
                direction,
                forceMultiplier,
                triggerLifetime,
                velocityScale
            );
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !_hasSpawned)
                return;

            // Draw spawn interval radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_lastSpawnPosition, _spawnInterval);

#if UNITY_EDITOR
            if (_showDebugInfo && _velocityTracker != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.cyan;
                style.fontSize = 12;

                string debugText = $"Speed: {_velocityTracker.Speed:F0} px/s\n" +
                                   $"Normalized: {_velocityTracker.NormalizedSpeed:F2}\n" +
                                   $"Force: {_lastForceApplied:F2}";

                UnityEditor.Handles.Label(
                    _lastSpawnPosition + Vector3.up * 1.5f,
                    debugText,
                    style
                );
            }
#endif
        }
    }
}