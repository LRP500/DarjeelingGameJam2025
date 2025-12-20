using System.Collections.Generic;
using System.Threading.Tasks;
using DarjeelingGameJam.Spores;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// Système de vent avec un seul trigger qui suit la souris.
    /// La taille et la force augmentent avec la vitesse pour un effet "ballon".
    /// Gère aussi les trails visuels multiples selon la vitesse.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class WindTracer : MonoBehaviour
    {
        [Header("References")]
        [Required]
        [SerializeField]
        private MouseVelocityTracker _velocityTracker;

        [Required]
        [SerializeField]
        private WindParticleController _particleController;

        [Header("Trigger Settings")]
        [Tooltip("Taille minimale du trigger (même à l'arrêt)")]
        [MinValue(0.1f)]
        [SerializeField]
        private float _minTriggerRadius = 2f;

        [Tooltip("Taille maximale du trigger (vitesse max)")]
        [MinValue(0.1f)]
        [SerializeField]
        private float _maxTriggerRadius = 7f;

        [Tooltip("Force minimale (même à l'arrêt)")]
        [MinValue(0f)]
        [SerializeField]
        private float _minForce = 0.5f;

        [Tooltip("Force maximale (vitesse max)")]
        [MinValue(0f)]
        [SerializeField]
        private float _maxForce = 1f;

        [Tooltip("Force continue (effet ballon)")]
        [SerializeField]
        private bool _continuousForce = true;

        [ShowIf(nameof(_continuousForce))]
        [Tooltip("Multiplicateur de force continue")]
        [MinValue(0f)]
        [SerializeField]
        private float _continuousForceMultiplier = 1.5f;

        [Header("Plant Wind Settings")]
        [Tooltip("Durée pendant laquelle le vent reste actif après que la plante sorte du trigger (évite le clignotement)")]
        [MinValue(0f)]
        [SerializeField]
        private float _plantWindDuration = 1f;

        [Header("Multi-Trails Settings")]
        [Tooltip("Activer les trails multiples")]
        [SerializeField]
        private bool _enableMultiTrails = true;

        [ShowIf(nameof(_enableMultiTrails))]
        [BoxGroup("Trails")]
        [Tooltip("Nombre max de trails")]
        [Range(1, 10)]
        [SerializeField]
        private int _maxTrails = 5;

        [ShowIf(nameof(_enableMultiTrails))]
        [BoxGroup("Trails")]
        [Tooltip("Trail de référence (copié pour créer les autres)")]
        [Required]
        [SerializeField]
        private TrailRenderer _referenceTrail;

        [ShowIf(nameof(_enableMultiTrails))]
        [BoxGroup("Trails")]
        [Tooltip("Courbe : vitesse (0-1) → nombre de trails (0-1). Permet un effet exponentiel.")]
        [SerializeField]
        private AnimationCurve _trailSpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [ShowIf(nameof(_enableMultiTrails))]
        [BoxGroup("Trails")]
        [Tooltip("Dispersion aléatoire autour de la souris (world units)")]
        [SerializeField]
        private float _trailRandomOffset = 0.3f;

        [Header("Common Settings")]
        [Tooltip("Courbe vitesse → effet")]
        [SerializeField]
        private AnimationCurve _velocityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Debug")]
        [Tooltip("Afficher debug")]
        [SerializeField]
        private bool _showDebugInfo = true;

        // Private vars
        private Camera _camera;
        private CircleCollider2D _circleCollider;
        private Rigidbody2D _rigidbody;
        private Vector2 _currentDirection;
        private float _currentForce;
        private float _currentRadius;

        // Spores and plants tracking
        private HashSet<Spore> _sporesInTrigger = new HashSet<Spore>();
        private HashSet<LoopEndOfClip> _plantsInTrigger = new HashSet<LoopEndOfClip>();

        // Multi-trails
        private TrailRenderer[] _trails;
        private Vector3[] _trailOffsets;
        private int _activeTrailCount = 0;

        private void Awake()
        {
            _camera = Camera.main;
            _circleCollider = GetComponent<CircleCollider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();

            if (_circleCollider == null)
                _circleCollider = gameObject.AddComponent<CircleCollider2D>();

            if (_rigidbody == null)
                _rigidbody = gameObject.AddComponent<Rigidbody2D>();

            _circleCollider.isTrigger = true;

            // Configure Rigidbody2D pour qu'il ne soit pas affecté par la physique
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.gravityScale = 0f;

            if (_enableMultiTrails)
                InitializeTrailPool();
        }

        private void Update()
        {
            if (_velocityTracker == null)
                return;

            // Get mouse position
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, _camera.nearClipPlane)
            );
            mouseWorldPos.z = 0;

            // Get velocity data
            Vector2 direction = _velocityTracker.GetDirection();
            float velocityNormalized = _velocityTracker.NormalizedSpeed;
            bool isMoving = _velocityTracker.IsMoving;

            // Update particles
            if (_particleController != null)
                _particleController.UpdateWindEmission(mouseWorldPos, direction, velocityNormalized, isMoving);

            // Update multi-trails
            if (_enableMultiTrails)
            {
                UpdateMultiTrails(mouseWorldPos, velocityNormalized);
            }

            // Update wind follower
            UpdateFollowerMode(mouseWorldPos, direction, velocityNormalized);
        }

        #region Wind Physics

        private void UpdateFollowerMode(Vector3 mouseWorldPos, Vector2 direction, float velocityNormalized)
        {
            // Move trigger to mouse position
            transform.position = mouseWorldPos;

            // Update size
            float sizeCurveValue = _velocityCurve.Evaluate(velocityNormalized);
            _currentRadius = Mathf.Lerp(_minTriggerRadius, _maxTriggerRadius, sizeCurveValue);
            _circleCollider.radius = _currentRadius;

            // Update force
            float forceCurveValue = _velocityCurve.Evaluate(velocityNormalized);
            _currentForce = Mathf.Lerp(_minForce, _maxForce, forceCurveValue);
            _currentDirection = direction;

            // Apply continuous force
            if (_continuousForce)
            {
                foreach (var spore in _sporesInTrigger)
                {
                    if (spore == null || !spore.IsDetached) continue;

                    Rigidbody2D rb = spore.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 forceDirection;

                        // Si la souris bouge, utiliser la direction du mouvement
                        if (_currentDirection.magnitude > 0.01f)
                        {
                            forceDirection = _currentDirection;
                        }
                        else
                        {
                            // Si immobile, repousser vers l'extérieur (depuis le centre du trigger vers la spore)
                            Vector2 toSpore = (spore.transform.position - transform.position);
                            forceDirection = toSpore.normalized;
                        }

                        Vector2 force = forceDirection * _currentForce * _continuousForceMultiplier;
                        rb.AddForce(force, ForceMode2D.Force);
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Spore"))
            {
                HandleSporeEnter(other);
            }
            else if (other.CompareTag("Plant"))
            {
                HandlePlantEnter(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Spore"))
            {
                var spore = other.GetComponent<Spore>();
                if (spore != null)
                    _sporesInTrigger.Remove(spore);
            }
            else if (other.CompareTag("Plant"))
            {
                var loopEndOfClip = other.GetComponentInParent<LoopEndOfClip>();
                if (loopEndOfClip != null)
                {
                    _plantsInTrigger.Remove(loopEndOfClip);
                    // Désactiver le vent après un délai
                    DeactivatePlantWindAfterDelay(loopEndOfClip);
                }
            }
        }

        private void HandleSporeEnter(Collider2D other)
        {
            var spore = other.GetComponent<Spore>();
            if (spore == null) return;

            // Détacher systématiquement les spores
            if (!spore.IsDetached)
            {
                spore.Detach();
            }

            if (spore.IsDetached)
            {
                _sporesInTrigger.Add(spore);

                if (!_continuousForce)
                {
                    Rigidbody2D rb = other.attachedRigidbody;
                    if (rb != null)
                        rb.AddForce(_currentDirection * _currentForce, ForceMode2D.Impulse);
                }
            }
        }

        private void HandlePlantEnter(Collider2D other)
        {
            var loopEndOfClip = other.GetComponentInParent<LoopEndOfClip>();
            if (loopEndOfClip == null)
                return;

            _plantsInTrigger.Add(loopEndOfClip);
            loopEndOfClip.windActive = true;
        }

        private async void DeactivatePlantWindAfterDelay(LoopEndOfClip loopEndOfClip)
        {
            if (loopEndOfClip == null)
                return;

            // Attendre la durée configurée
            await Task.Delay(System.TimeSpan.FromSeconds(_plantWindDuration));

            // Désactiver seulement si la plante n'est pas revenue dans le trigger entre-temps
            if (loopEndOfClip != null && !_plantsInTrigger.Contains(loopEndOfClip))
            {
                loopEndOfClip.windActive = false;
            }
        }

        #endregion

        #region Multi-Trails

        private void InitializeTrailPool()
        {
            if (_referenceTrail == null)
            {
                Debug.LogError("[WindTracer] Reference Trail manquant ! Glissez votre trail existant dans ce champ.", this);
                return;
            }

            _trails = new TrailRenderer[_maxTrails];
            _trailOffsets = new Vector3[_maxTrails];

            // Le trail 0 est le trail de référence lui-même
            _trails[0] = _referenceTrail;
            _trailOffsets[0] = Vector3.zero;

            // Créer les trails supplémentaires comme des copies du trail de référence
            for (int i = 1; i < _maxTrails; i++)
            {
                // Dupliquer le GameObject du trail de référence
                GameObject trailObj = Instantiate(_referenceTrail.gameObject, transform);
                trailObj.name = $"Trail_{i}";

                _trails[i] = trailObj.GetComponent<TrailRenderer>();

                if (_trails[i] != null)
                {
                    // Calculer un offset aléatoire pour disperser les trails
                    CalculateTrailOffset(i);
                    _trails[i].emitting = false;
                }
            }
        }

        private void CalculateTrailOffset(int index)
        {
            // Offset aléatoire autour de la souris
            float randomX = Random.Range(-_trailRandomOffset, _trailRandomOffset);
            float randomY = Random.Range(-_trailRandomOffset, _trailRandomOffset);
            _trailOffsets[index] = new Vector3(randomX, randomY, 0f);
        }

        private void UpdateMultiTrails(Vector3 mouseWorldPos, float normalizedSpeed)
        {
            if (_trails == null) return;

            // Calculate active trail count using the curve
            // Utiliser la courbe pour mapper la vitesse au nombre de trails
            float curveValue = _trailSpeedCurve.Evaluate(normalizedSpeed);
            int targetTrailCount = Mathf.RoundToInt(Mathf.Lerp(1, _maxTrails, curveValue));

            if (targetTrailCount != _activeTrailCount)
            {
                _activeTrailCount = targetTrailCount;
                for (int i = 0; i < _maxTrails; i++)
                {
                    if (_trails[i] != null)
                        _trails[i].emitting = i < _activeTrailCount;
                }
            }

            // Update positions - dispersion aléatoire autour de la souris
            for (int i = 0; i < _activeTrailCount; i++)
            {
                if (_trails[i] != null)
                {
                    // Position = souris + offset aléatoire
                    _trails[i].transform.position = mouseWorldPos + _trailOffsets[i];
                }
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_showDebugInfo || !Application.isPlaying) return;

            Gizmos.color = _velocityTracker != null && _velocityTracker.IsMoving
                ? new Color(0, 1, 1, 0.3f)
                : new Color(1, 1, 0, 0.2f);

            Gizmos.DrawWireSphere(transform.position, _currentRadius);

            if (_velocityTracker != null && _velocityTracker.IsMoving)
            {
                Gizmos.color = Color.cyan;
                Vector3 directionIndicator = transform.position + (Vector3)_currentDirection * (_currentRadius + 0.5f);
                Gizmos.DrawLine(transform.position, directionIndicator);
            }

#if UNITY_EDITOR
            if (_velocityTracker != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.fontSize = 11;

                string stats = $"Radius: {_currentRadius:F2}\n" +
                              $"Force: {_currentForce:F2}\n" +
                              $"Trails: {_activeTrailCount}/{_maxTrails}\n" +
                              $"Spores: {_sporesInTrigger.Count}";

                UnityEditor.Handles.Label(transform.position + Vector3.up * (_currentRadius + 0.5f), stats, style);
            }
#endif
        }

        #endregion
    }
}
