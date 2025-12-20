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

        [Tooltip("Biais vertical vers le haut (0 = neutre, 1 = toujours monter)")]
        [Range(0f, 2f)]
        [SerializeField]
        private float _upwardBias = 0.5f;

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
        [Tooltip("Prefabs des trails (glissez vos prefabs TrailRenderer avec des looks différents)")]
        [SerializeField]
        private TrailRenderer[] _trailPrefabs;

        [ShowIf(nameof(_enableMultiTrails))]
        [BoxGroup("Trails")]
        [Tooltip("Vitesse minimale (0-1) pour activer chaque trail. Index 0 = toujours visible, index 1 = apparaît à cette vitesse, etc.")]
        [SerializeField]
        private float[] _trailSpeedThresholds = new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f };

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
        private TrailRenderer[] _trailInstances;
        private Vector3[] _trailOffsets;

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
            // Continuous collision detection pour détecter les collisions même à haute vitesse
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

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

                        // Ajouter un biais vertical vers le haut
                        forceDirection.y += _upwardBias;
                        forceDirection.Normalize();

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
            if (_trailPrefabs == null || _trailPrefabs.Length == 0)
            {
                Debug.LogWarning("[WindTracer] Aucun prefab de trail assigné ! Glissez vos prefabs TrailRenderer dans le tableau.", this);
                return;
            }

            _trailInstances = new TrailRenderer[_trailPrefabs.Length];
            _trailOffsets = new Vector3[_trailPrefabs.Length];

            // Instancier chaque prefab de trail et calculer un offset aléatoire
            for (int i = 0; i < _trailPrefabs.Length; i++)
            {
                if (_trailPrefabs[i] != null)
                {
                    // Instancier le prefab comme enfant de WindTracer
                    GameObject trailObj = Instantiate(_trailPrefabs[i].gameObject, transform);
                    trailObj.name = $"Trail_{i}";
                    _trailInstances[i] = trailObj.GetComponent<TrailRenderer>();

                    CalculateTrailOffset(i);
                    _trailInstances[i].emitting = false;
                }
            }
        }

        private void CalculateTrailOffset(int index)
        {
            // Le premier trail (index 0) suit exactement la souris, les autres ont un offset
            if (index == 0)
            {
                _trailOffsets[index] = Vector3.zero;
            }
            else
            {
                // Offset aléatoire autour de la souris pour les trails supplémentaires
                float randomX = Random.Range(-_trailRandomOffset, _trailRandomOffset);
                float randomY = Random.Range(-_trailRandomOffset, _trailRandomOffset);
                _trailOffsets[index] = new Vector3(randomX, randomY, 0f);
            }
        }

        private void UpdateMultiTrails(Vector3 mouseWorldPos, float normalizedSpeed)
        {
            if (_trailInstances == null || _trailInstances.Length == 0) return;

            // Activer/désactiver chaque trail selon son seuil de vitesse
            for (int i = 0; i < _trailInstances.Length; i++)
            {
                if (_trailInstances[i] != null)
                {
                    // Récupérer le seuil pour ce trail (si pas défini, utiliser un défaut)
                    float threshold = i < _trailSpeedThresholds.Length ? _trailSpeedThresholds[i] : (i * 0.2f);

                    // Activer si la vitesse dépasse le seuil
                    bool shouldEmit = normalizedSpeed >= threshold;
                    _trailInstances[i].emitting = shouldEmit;

                    // Mettre à jour la position si actif
                    if (shouldEmit)
                    {
                        _trailInstances[i].transform.position = mouseWorldPos + _trailOffsets[i];
                    }
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

                int activeTrails = 0;
                if (_trailInstances != null)
                {
                    foreach (var trail in _trailInstances)
                    {
                        if (trail != null && trail.emitting) activeTrails++;
                    }
                }

                string stats = $"Radius: {_currentRadius:F2}\n" +
                              $"Force: {_currentForce:F2}\n" +
                              $"Trails: {activeTrails}/{(_trailInstances != null ? _trailInstances.Length : 0)}\n" +
                              $"Spores: {_sporesInTrigger.Count}";

                UnityEditor.Handles.Label(transform.position + Vector3.up * (_currentRadius + 0.5f), stats, style);
            }
#endif
        }

        #endregion
    }
}
