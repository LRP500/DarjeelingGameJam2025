using Sirenix.OdinInspector;
using UnityEngine;

namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// Controls particle emission to create a trail effect when cursor moves.
    /// </summary>
    public class WindParticleController : MonoBehaviour
    {
        [Header("Particle System")]
        [Required]
        [Tooltip("ParticleSystem that emits particles when moving")]
        [SerializeField]
        private ParticleSystem _particleSystem;

        [Header("Emission Settings")]
        [Tooltip("Emission rate when moving")]
        [MinValue(0f)]
        [SerializeField]
        private float _emissionRate = 100f;

        [Header("Trail Renderer (Optional)")]
        [Tooltip("Optional TrailRenderer that follows cursor")]
        [SerializeField]
        private TrailRenderer _trailRenderer;

        [Tooltip("Smoothing time for trail position (lower = tighter curves, higher = smoother but more lag)")]
        [Range(0.02f, 0.06f)]
        [SerializeField]
        private float _trailSmoothTime = 0.03f;

        [Tooltip("Delay before stopping trail emission after movement stops (prevents micro-cuts)")]
        [Range(0.01f, 0.2f)]
        [SerializeField]
        private float _stopDelay = 0.05f;

        private ParticleSystem.EmissionModule _emission;
        private Vector3 _trailTargetPosition;
        private Vector3 _trailVelocity;
        private float _lastMoveTime;

        private void Awake()
        {
            if (_particleSystem != null)
            {
                _emission = _particleSystem.emission;
                _emission.rateOverTime = 0f;
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
            }
        }

        /// <summary>
        /// Updates particle emission and position based on mouse movement
        /// </summary>
        public void UpdateWindEmission(
            Vector3 position,
            Vector2 direction,
            float velocityNormalized,
            bool isMoving)
        {
            if (_particleSystem == null)
                return;

            // Always update position to follow cursor
            _particleSystem.transform.position = position;

            // Control emission based on movement
            if (isMoving)
            {
                _emission.rateOverTime = _emissionRate;
            }
            else
            {
                _emission.rateOverTime = 0f;
            }

            // Update trail renderer if present
            if (_trailRenderer != null)
            {
                // Smooth movement for trail using SmoothDamp for natural curves
                _trailTargetPosition = position;
                _trailRenderer.transform.position = Vector3.SmoothDamp(
                    _trailRenderer.transform.position,
                    _trailTargetPosition,
                    ref _trailVelocity,
                    _trailSmoothTime
                );

                // Track last movement time for hysteresis
                if (isMoving)
                {
                    _lastMoveTime = Time.time;
                }

                // Use delay to prevent micro-cuts when movement oscillates
                bool shouldEmit = (Time.time - _lastMoveTime) <= _stopDelay;

                if (shouldEmit && !_trailRenderer.emitting)
                {
                    _trailRenderer.emitting = true;
                    // Removed Clear() to prevent visual cuts
                }
                else if (!shouldEmit && _trailRenderer.emitting)
                {
                    _trailRenderer.emitting = false;
                }
            }
        }
    }
}
