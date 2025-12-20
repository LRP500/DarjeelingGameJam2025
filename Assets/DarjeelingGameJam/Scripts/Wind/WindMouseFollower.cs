using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DarjeelingGameJam.Spores;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// UN SEUL trigger de vent qui suit la souris en temps réel.
    /// Sa taille et sa force augmentent avec la vitesse de la souris.
    /// Permet de "jouer" avec les spores comme des ballons volants.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class WindMouseFollower : MonoBehaviour
    {
        [Header("References")]
        [Required]
        [Tooltip("Le MouseVelocityTracker pour connaître la vitesse")]
        [SerializeField]
        private MouseVelocityTracker _velocityTracker;

        [Header("Trigger Size")]
        [Tooltip("Taille minimale du trigger (quand immobile ou lent)")]
        [MinValue(0.1f)]
        [SerializeField]
        private float _minTriggerRadius = 0.5f;

        [Tooltip("Taille maximale du trigger (quand très rapide)")]
        [MinValue(0.1f)]
        [SerializeField]
        private float _maxTriggerRadius = 2.5f;

        [Tooltip("Courbe pour mapper la vitesse (0-1) vers la taille du trigger")]
        [SerializeField]
        private AnimationCurve _sizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Wind Force")]
        [Tooltip("Force minimale appliquée aux spores (quand lent)")]
        [MinValue(0f)]
        [SerializeField]
        private float _minForce = 1f;

        [Tooltip("Force maximale appliquée aux spores (quand très rapide)")]
        [MinValue(0f)]
        [SerializeField]
        private float _maxForce = 15f;

        [Tooltip("Courbe pour mapper la vitesse (0-1) vers la force")]
        [SerializeField]
        private AnimationCurve _forceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Spore Detachment")]
        [Tooltip("Chance de détacher une spore attachée (0-1)")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _sporeDetachChance = 0.5f;

        [Tooltip("Vitesse minimale pour détacher les spores")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _minSpeedToDetach = 0.2f;

        [Header("Plant Effect")]
        [Tooltip("Durée pendant laquelle le vent affecte une plante")]
        [MinValue(0f)]
        [SerializeField]
        private float _plantEffectDuration = 2f;

        [Header("Force Application")]
        [Tooltip("Appliquer la force en continu (effet ballon) ou juste à l'entrée")]
        [SerializeField]
        private bool _continuousForce = true;

        [Tooltip("Force appliquée par seconde en mode continu")]
        [MinValue(0f)]
        [SerializeField]
        private float _continuousForceMultiplier = 5f;

        [Header("Behavior")]
        [Tooltip("Désactiver le trigger quand la souris ne bouge pas")]
        [SerializeField]
        private bool _disableWhenIdle = false;

        [Tooltip("Vitesse minimale pour activer le trigger (0-1)")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _minSpeedToActivate = 0.05f;

        [Header("Debug")]
        [Tooltip("Afficher les Gizmos de debug")]
        [SerializeField]
        private bool _showDebugGizmos = true;

        private Camera _mainCamera;
        private CircleCollider2D _circleCollider;
        private Vector2 _currentDirection;
        private float _currentForce;
        private float _currentRadius;

        // Cache pour les spores dans le trigger
        private HashSet<Spore> _sporesInTrigger = new HashSet<Spore>();
        private HashSet<LoopEndOfClip> _plantsInTrigger = new HashSet<LoopEndOfClip>();

        private void Awake()
        {
            _mainCamera = Camera.main;
            _circleCollider = GetComponent<CircleCollider2D>();

            if (_circleCollider == null)
            {
                _circleCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            _circleCollider.isTrigger = true;
        }

        private void Update()
        {
            if (_velocityTracker == null)
            {
                Debug.LogWarning("[WindMouseFollower] MouseVelocityTracker manquant !", this);
                return;
            }

            UpdatePosition();
            UpdateTriggerSize();
            UpdateForce();

            if (_continuousForce)
            {
                ApplyContinuousForce();
            }
        }

        private void UpdatePosition()
        {
            // Suivre la souris en temps réel
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, _mainCamera.nearClipPlane)
            );
            mouseWorldPos.z = 0f;

            transform.position = mouseWorldPos;
        }

        private void UpdateTriggerSize()
        {
            float normalizedSpeed = _velocityTracker.NormalizedSpeed;

            // Appliquer la courbe de taille
            float sizeCurveValue = _sizeCurve.Evaluate(normalizedSpeed);
            _currentRadius = Mathf.Lerp(_minTriggerRadius, _maxTriggerRadius, sizeCurveValue);

            _circleCollider.radius = _currentRadius;

            // Désactiver le trigger si la souris ne bouge pas (optionnel)
            if (_disableWhenIdle)
            {
                _circleCollider.enabled = normalizedSpeed >= _minSpeedToActivate;
            }
        }

        private void UpdateForce()
        {
            float normalizedSpeed = _velocityTracker.NormalizedSpeed;

            // Appliquer la courbe de force
            float forceCurveValue = _forceCurve.Evaluate(normalizedSpeed);
            _currentForce = Mathf.Lerp(_minForce, _maxForce, forceCurveValue);

            // Direction du mouvement
            _currentDirection = _velocityTracker.GetDirection();
        }

        private void ApplyContinuousForce()
        {
            // Appliquer la force à toutes les spores dans le trigger
            foreach (var spore in _sporesInTrigger)
            {
                if (spore == null || !spore.IsDetached) continue;

                Rigidbody2D rb = spore.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Force continue proportionnelle au deltaTime
                    Vector2 force = _currentDirection * _currentForce * _continuousForceMultiplier * Time.deltaTime;
                    rb.AddForce(force, ForceMode2D.Force);
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

        private void OnTriggerStay2D(Collider2D other)
        {
            // En mode continu, on met à jour dans ApplyContinuousForce()
            // Ici on peut juste s'assurer que les plantes restent activées
            if (other.CompareTag("Plant"))
            {
                var loopEndOfClip = other.GetComponent<LoopEndOfClip>();
                if (loopEndOfClip != null && !_plantsInTrigger.Contains(loopEndOfClip))
                {
                    HandlePlantEnter(other);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Spore"))
            {
                var spore = other.GetComponent<Spore>();
                if (spore != null)
                {
                    _sporesInTrigger.Remove(spore);
                }
            }
            else if (other.CompareTag("Plant"))
            {
                var loopEndOfClip = other.GetComponent<LoopEndOfClip>();
                if (loopEndOfClip != null)
                {
                    _plantsInTrigger.Remove(loopEndOfClip);
                }
            }
        }

        private void HandleSporeEnter(Collider2D other)
        {
            var spore = other.GetComponent<Spore>();
            if (spore == null) return;

            // Détacher la spore si elle est attachée (selon la chance et la vitesse)
            if (!spore.IsDetached)
            {
                float normalizedSpeed = _velocityTracker.NormalizedSpeed;

                if (normalizedSpeed >= _minSpeedToDetach && Random.value < _sporeDetachChance)
                {
                    spore.Detach();
                }
            }

            // Ajouter au cache si détachée
            if (spore.IsDetached)
            {
                _sporesInTrigger.Add(spore);

                // Appliquer une force initiale (impulse) si pas en mode continu
                if (!_continuousForce)
                {
                    Rigidbody2D rb = other.attachedRigidbody;
                    if (rb != null)
                    {
                        rb.AddForce(_currentDirection * _currentForce, ForceMode2D.Impulse);
                    }
                }
            }
        }

        private void HandlePlantEnter(Collider2D other)
        {
            var loopEndOfClip = other.GetComponent<LoopEndOfClip>();
            if (loopEndOfClip == null) return;

            _plantsInTrigger.Add(loopEndOfClip);
            ActivatePlantWind(loopEndOfClip).Forget();
        }

        private async UniTask ActivatePlantWind(LoopEndOfClip loopEndOfClip)
        {
            if (loopEndOfClip == null) return;

            loopEndOfClip.windActive = true;

            // Attendre la durée de l'effet (utilise Task.Delay comme dans WindForceTrigger)
            await System.Threading.Tasks.Task.Delay(
                System.TimeSpan.FromSeconds(_plantEffectDuration));

            // Désactiver seulement si la plante n'est plus dans le trigger
            if (loopEndOfClip != null && !_plantsInTrigger.Contains(loopEndOfClip))
            {
                loopEndOfClip.windActive = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos || !Application.isPlaying) return;

            // Dessiner le cercle du trigger
            Gizmos.color = _velocityTracker != null && _velocityTracker.IsMoving
                ? new Color(0, 1, 1, 0.3f) // Cyan transparent quand actif
                : new Color(1, 1, 0, 0.2f); // Jaune transparent quand inactif

            Gizmos.DrawWireSphere(transform.position, _currentRadius);

            // Dessiner la direction du vent
            if (_velocityTracker != null && _velocityTracker.IsMoving)
            {
                Gizmos.color = Color.cyan;
                Vector3 directionIndicator = transform.position + (Vector3)_currentDirection * (_currentRadius + 0.5f);
                Gizmos.DrawLine(transform.position, directionIndicator);

                // Flèche
                DrawArrow(transform.position, directionIndicator, 0.3f);
            }

#if UNITY_EDITOR
            // Afficher les stats
            if (_velocityTracker != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.fontSize = 11;

                string stats = $"Radius: {_currentRadius:F2}\n" +
                              $"Force: {_currentForce:F2}\n" +
                              $"Speed: {_velocityTracker.NormalizedSpeed:F2}\n" +
                              $"Spores: {_sporesInTrigger.Count}";

                UnityEditor.Handles.Label(transform.position + Vector3.up * (_currentRadius + 0.5f), stats, style);
            }
#endif
        }

        private void DrawArrow(Vector3 from, Vector3 to, float arrowSize)
        {
            Vector3 direction = (to - from).normalized;
            Vector3 right = Vector3.Cross(Vector3.forward, direction) * arrowSize;
            Vector3 arrowPoint1 = to - direction * arrowSize + right;
            Vector3 arrowPoint2 = to - direction * arrowSize - right;

            Gizmos.DrawLine(to, arrowPoint1);
            Gizmos.DrawLine(to, arrowPoint2);
        }

        // API publique pour désactiver/activer le système
        public void SetEnabled(bool enabled)
        {
            _circleCollider.enabled = enabled;
        }

        public float CurrentRadius => _currentRadius;
        public float CurrentForce => _currentForce;
        public int SporesInTrigger => _sporesInTrigger.Count;
    }
}
