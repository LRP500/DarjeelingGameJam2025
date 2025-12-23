using System.Collections;
using System.Collections.Generic;
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

        [Tooltip("AudioSource pour jouer le son de vent (optionnel)")]
        [SerializeField]
        private AudioSource _windAudioSource;

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

        [Tooltip("Angle aléatoire maximal ajouté à la direction (en degrés, pour disperser les spores)")]
        [Range(0f, 45f)]
        [SerializeField]
        private float _maxDirectionalVariation = 15f;

        [Header("Plant Wind Settings")]
        [Tooltip("Durée pendant laquelle le vent reste actif après que la plante sorte du trigger (évite le clignotement)")]
        [MinValue(0f)]
        [SerializeField]
        private float _plantWindDuration = 1f;

        [Tooltip("Fréquence de scan des plantes (en secondes). 0.15s = ~7 fois/sec")]
        [MinValue(0.05f)]
        [SerializeField]
        private float _plantScanInterval = 0.15f;

        [Tooltip("Rayon d'entrée du vent pour les plantes (utilise le trigger radius actuel)")]
        [MinValue(0.1f)]
        [SerializeField]
        private float _plantEnterRadius = 3f;

        [Tooltip("Rayon de sortie du vent (légèrement plus grand pour éviter le flicker)")]
        [MinValue(0.1f)]
        [SerializeField]
        private float _plantExitRadius = 3.3f;

        [Tooltip("Activer le falloff de force selon la distance")]
        [SerializeField]
        private bool _useFalloff = true;

        [Header("Wind Sound Settings")]
        [Tooltip("Vitesse normalisée minimum (0-1) pour jouer le son de vent")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _windSoundSpeedThreshold = 0.7f;

        [Tooltip("Délai minimum (en secondes) avant de pouvoir rejouer le son")]
        [MinValue(0f)]
        [SerializeField]
        private float _windSoundCooldown = 2f;

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
        private float[] _trailSpeedThresholds = new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.4f };

        [ShowIf(nameof(_enableMultiTrails))]
        [BoxGroup("Trails")]
        [Tooltip("Vitesse du fade in/out des trails (plus petit = plus smooth)")]
        [Range(0.01f, 1f)]
        [SerializeField]
        private float _trailFadeSpeed = 0.1f;

        [ShowIf(nameof(_enableMultiTrails))]
        [BoxGroup("Trails")]
        [Tooltip("Offset manuel pour chaque trail (en world units). Laisse vide pour générer aléatoirement.")]
        [SerializeField]
        private Vector3[] _trailManualOffsets = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),      // Trail 0: centre
            new Vector3(0f, -0.3f, 0f),   // Trail 1: bas
            new Vector3(0f, 0.3f, 0f),    // Trail 2: haut
            new Vector3(-0.3f, 0f, 0f),   // Trail 3: gauche
            new Vector3(0.3f, 0f, 0f)     // Trail 4: droite
        };

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
        private bool _initialized = false;

        // Spores tracking (continue d'utiliser Physics2D)
        private HashSet<Spore> _sporesInTrigger = new HashSet<Spore>();

        // Plant tracking (utilise la Spatial Grid au lieu de Physics2D)
        private readonly HashSet<int> _plantsInsideWind = new HashSet<int>(256);
        private readonly List<int> _plantCandidates = new List<int>(512);
        private readonly List<int> _plantsToRemove = new List<int>(256);
        private float _plantEnterSqr;
        private float _plantExitSqr;

        // Multi-trails
        private TrailRenderer[] _trailInstances;
        private Vector3[] _trailOffsets;
        private float[] _trailCurrentAlpha; // Alpha courant de chaque trail pour smooth fade
        private Gradient[] _trailOriginalGradient; // Gradient original de chaque trail
        private Gradient[] _trailCachedGradient; // Gradient réutilisable pour éviter les allocations
        private GradientAlphaKey[][] _trailCachedAlphaKeys; // Alpha keys réutilisables

        // Wind sound
        private float _lastWindSoundTime = -999f;

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

            // Désactiver le collider au début pour éviter les collisions pendant l'initialisation
            _circleCollider.enabled = false;

            // Configure Rigidbody2D pour qu'il ne soit pas affecté par la physique
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.gravityScale = 0f;
            // Continuous collision detection pour détecter les collisions même à haute vitesse
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Initialiser la taille et la force au minimum
            _currentRadius = _minTriggerRadius;
            _circleCollider.radius = _currentRadius;
            _currentForce = _minForce;
            _currentDirection = Vector2.zero;

            // Initialiser les rayons carrés pour le scan des plantes (évite sqrt)
            if (_plantExitRadius < _plantEnterRadius)
                _plantExitRadius = _plantEnterRadius * 1.1f;
            _plantEnterSqr = _plantEnterRadius * _plantEnterRadius;
            _plantExitSqr = _plantExitRadius * _plantExitRadius;
        }

        private void OnDestroy()
        {
            // Nettoyer les trails quand l'objet est détruit
            if (_trailInstances != null)
            {
                foreach (var trail in _trailInstances)
                {
                    if (trail != null)
                        Destroy(trail.gameObject);
                }
            }
        }

        private void Start()
        {
            StartCoroutine(InitializeCoroutine());
        }

        private IEnumerator InitializeCoroutine()
        {
            // Attendre un court délai pour que tout soit bien initialisé
            yield return new WaitForSeconds(0.1f);

            if (_enableMultiTrails)
                InitializeTrailPool();

            // Attendre 2 secondes avant d'activer le système
            yield return new WaitForSeconds(2f);

            _initialized = true;
            _circleCollider.enabled = true;

            // Démarrer le scan périodique des plantes via la Spatial Grid
            StartCoroutine(PlantScanLoop());
        }

        private void Update()
        {
            if (!_initialized || _velocityTracker == null)
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

            // Check wind sound
            TryPlayWindSound(velocityNormalized);

            // Update wind follower FIRST (move parent before children)
            UpdateFollowerMode(mouseWorldPos, direction, velocityNormalized);

            // Update multi-trails AFTER (trails use localPosition relative to parent)
            if (_enableMultiTrails)
            {
                UpdateMultiTrails(mouseWorldPos, velocityNormalized);
            }
        }

        private void FixedUpdate()
        {
            if (!_initialized || !_continuousForce)
                return;

            // Apply physics forces in FixedUpdate for frame-rate independence
            ApplyWindForces();
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

        }

        private void ApplyWindForces()
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

                    // Ajouter un biais vertical vers le haut, modulé par la position verticale
                    // Plus on est haut à l'écran, moins le bias est fort
                    float sporeScreenY = _camera.WorldToViewportPoint(spore.transform.position).y;
                    float upwardBiasMultiplier = Mathf.Clamp01(1f - sporeScreenY); // 1 en bas, 0 en haut
                    forceDirection.y += _upwardBias * upwardBiasMultiplier;
                    forceDirection.Normalize();

                    // Ajouter une variation angulaire aléatoire pour disperser les spores
                    if (_maxDirectionalVariation > 0f)
                    {
                        float randomAngle = Random.Range(-_maxDirectionalVariation, _maxDirectionalVariation);
                        float angleRad = randomAngle * Mathf.Deg2Rad;
                        float cos = Mathf.Cos(angleRad);
                        float sin = Mathf.Sin(angleRad);
                        forceDirection = new Vector2(
                            forceDirection.x * cos - forceDirection.y * sin,
                            forceDirection.x * sin + forceDirection.y * cos
                        );
                    }

                    Vector2 force = forceDirection * _currentForce * _continuousForceMultiplier;
                    rb.AddForce(force, ForceMode2D.Force);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ne gère plus que les spores (les plantes utilisent la Spatial Grid)
            if (other.CompareTag("Spore"))
            {
                HandleSporeEnter(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Ne gère plus que les spores (les plantes utilisent la Spatial Grid)
            if (other.CompareTag("Spore"))
            {
                var spore = other.GetComponent<Spore>();
                if (spore != null)
                    _sporesInTrigger.Remove(spore);
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

                if (!_continuousForce && _initialized)
                {
                    Rigidbody2D rb = other.attachedRigidbody;
                    if (rb != null)
                    {
                        Vector2 forceDirection = _currentDirection;

                        // Ajouter une variation angulaire aléatoire pour disperser les spores
                        if (_maxDirectionalVariation > 0f)
                        {
                            float randomAngle = Random.Range(-_maxDirectionalVariation, _maxDirectionalVariation);
                            float angleRad = randomAngle * Mathf.Deg2Rad;
                            float cos = Mathf.Cos(angleRad);
                            float sin = Mathf.Sin(angleRad);
                            forceDirection = new Vector2(
                                forceDirection.x * cos - forceDirection.y * sin,
                                forceDirection.x * sin + forceDirection.y * cos
                            );
                        }

                        rb.AddForce(forceDirection * _currentForce, ForceMode2D.Impulse);
                    }
                }
            }
        }

        /// <summary>
        /// Scan périodique des plantes via la Spatial Grid.
        /// Remplace OnTriggerEnter/Exit pour les plantes (beaucoup plus optimisé).
        /// </summary>
        private IEnumerator PlantScanLoop()
        {
            var wait = new WaitForSeconds(_plantScanInterval);

            while (enabled)
            {
                if (PlantSpatialGrid.Instance != null)
                {
                    ScanPlantsInRadius();
                }

                yield return wait;
            }
        }

        /// <summary>
        /// Scan les plantes dans le rayon du vent et met à jour leur état.
        /// </summary>
        private void ScanPlantsInRadius()
        {
            var grid = PlantSpatialGrid.Instance;
            Vector2 center = transform.position;

            // 1) Query la grille pour obtenir les candidates dans les cellules autour
            grid.Query(center, _plantEnterRadius, _plantCandidates);

            // 2) Filtrage fin : Enter/Stay (vérifier distance exacte avec sqrMagnitude)
            for (int i = 0; i < _plantCandidates.Count; i++)
            {
                int plantId = _plantCandidates[i];
                Vector2 plantPos = grid.GetPlantPosition(plantId);
                Vector2 delta = plantPos - center;
                float distSqr = delta.sqrMagnitude;

                // Si dans le rayon d'entrée, activer le vent
                if (distSqr <= _plantEnterSqr)
                {
                    // Ajouter à la liste des plantes actives
                    _plantsInsideWind.Add(plantId);

                    // Calculer la force (falloff optionnel)
                    float strength = 1f;
                    if (_useFalloff && _plantEnterSqr > 0f)
                    {
                        strength = 1f - Mathf.Clamp01(distSqr / _plantEnterSqr);
                    }

                    // Activer le vent sur la plante
                    var windComponent = grid.GetPlantWindComponent(plantId);
                    windComponent?.SetWind(true, strength);
                }
            }

            // 3) Exit (hystérésis) : vérifier les plantes qui étaient inside et sont maintenant outside
            _plantsToRemove.Clear();
            foreach (var plantId in _plantsInsideWind)
            {
                Vector2 plantPos = grid.GetPlantPosition(plantId);
                Vector2 delta = plantPos - center;
                float distSqr = delta.sqrMagnitude;

                // Si sorti du rayon de sortie (hystérésis)
                if (distSqr > _plantExitSqr)
                {
                    _plantsToRemove.Add(plantId);
                }
            }

            // Retirer les plantes sorties et désactiver leur vent après un délai
            for (int i = 0; i < _plantsToRemove.Count; i++)
            {
                int plantId = _plantsToRemove[i];
                _plantsInsideWind.Remove(plantId);

                // Désactiver le vent après un délai (anti-flicker)
                StartCoroutine(DeactivatePlantWindAfterDelay(plantId));
            }
        }

        /// <summary>
        /// Désactive le vent sur une plante après un délai (évite le clignotement).
        /// </summary>
        private IEnumerator DeactivatePlantWindAfterDelay(int plantId)
        {
            // Attendre la durée configurée
            yield return new WaitForSeconds(_plantWindDuration);

            // Désactiver seulement si la plante n'est pas revenue dans le trigger entre-temps
            if (!_plantsInsideWind.Contains(plantId))
            {
                var grid = PlantSpatialGrid.Instance;
                if (grid != null)
                {
                    var windComponent = grid.GetPlantWindComponent(plantId);
                    windComponent?.SetWind(false, 0f);
                }
            }
        }

        #endregion

        #region Wind Sound

        private void TryPlayWindSound(float velocityNormalized)
        {
            // Vérifier si on a un AudioSource assigné
            if (_windAudioSource == null)
                return;

            // Vérifier si la vitesse dépasse le seuil
            if (velocityNormalized < _windSoundSpeedThreshold)
                return;

            // Vérifier si le son est déjà en train de jouer (pas de superposition)
            if (_windAudioSource.isPlaying)
                return;

            // Vérifier le cooldown (délai avant de pouvoir rejouer)
            float timeSinceLastSound = Time.time - _lastWindSoundTime;
            if (timeSinceLastSound < _windSoundCooldown)
                return;

            // Toutes les conditions sont remplies, jouer le son une fois
            _windAudioSource.PlayOneShot(_windAudioSource.clip);
            _lastWindSoundTime = Time.time;
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
            _trailCurrentAlpha = new float[_trailPrefabs.Length];
            _trailOriginalGradient = new Gradient[_trailPrefabs.Length];
            _trailCachedGradient = new Gradient[_trailPrefabs.Length];
            _trailCachedAlphaKeys = new GradientAlphaKey[_trailPrefabs.Length][];

            // Obtenir la position initiale de la souris
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, _camera.nearClipPlane)
            );
            mouseWorldPos.z = 0;

            // Positionner le WindTracer à la position de la souris dès le début
            transform.position = mouseWorldPos;

            // Instancier chaque prefab de trail et calculer un offset aléatoire
            for (int i = 0; i < _trailPrefabs.Length; i++)
            {
                if (_trailPrefabs[i] != null)
                {
                    // Calculer l'offset avant d'instancier
                    CalculateTrailOffset(i);

                    // Instancier le prefab SANS parent (world space) pour qu'il puisse bouger indépendamment
                    GameObject trailObj = Instantiate(_trailPrefabs[i].gameObject, mouseWorldPos + _trailOffsets[i], Quaternion.identity, null);
                    trailObj.name = $"Trail_{i}";
                    _trailInstances[i] = trailObj.GetComponent<TrailRenderer>();

                    // Copier le gradient original
                    _trailOriginalGradient[i] = new Gradient();
                    _trailOriginalGradient[i].SetKeys(
                        _trailInstances[i].colorGradient.colorKeys,
                        _trailInstances[i].colorGradient.alphaKeys
                    );

                    // Initialiser l'alpha à 0 (invisible)
                    _trailCurrentAlpha[i] = 0f;

                    // Créer les gradients et alpha keys cachés (alloués une seule fois)
                    _trailCachedGradient[i] = new Gradient();
                    _trailCachedAlphaKeys[i] = new GradientAlphaKey[_trailOriginalGradient[i].alphaKeys.Length];

                    // Initialiser les alpha keys à 0 (invisible au début)
                    for (int k = 0; k < _trailCachedAlphaKeys[i].Length; k++)
                    {
                        _trailCachedAlphaKeys[i][k] = new GradientAlphaKey(0f, _trailOriginalGradient[i].alphaKeys[k].time);
                    }
                    _trailCachedGradient[i].SetKeys(_trailOriginalGradient[i].colorKeys, _trailCachedAlphaKeys[i]);
                    _trailInstances[i].colorGradient = _trailCachedGradient[i];

                    // Activer le trail dès le début
                    _trailInstances[i].emitting = true;
                    _trailInstances[i].enabled = true;
                }
            }
        }

        private void CalculateTrailOffset(int index)
        {
            // Utiliser l'offset manuel si défini, sinon générer aléatoirement
            if (_trailManualOffsets != null && index < _trailManualOffsets.Length)
            {
                _trailOffsets[index] = _trailManualOffsets[index];
            }
            else
            {
                // Fallback: le premier trail au centre, les autres aléatoires
                if (index == 0)
                {
                    _trailOffsets[index] = Vector3.zero;
                }
                else
                {
                    float randomX = Random.Range(-0.3f, 0.3f);
                    float randomY = Random.Range(-0.3f, 0.3f);
                    _trailOffsets[index] = new Vector3(randomX, randomY, 0f);
                }
            }
        }

        private void UpdateMultiTrails(Vector3 mouseWorldPos, float normalizedSpeed)
        {
            if (_trailInstances == null || _trailInstances.Length == 0) return;

            // Suivre la souris en world space avec un smooth follow
            for (int i = 0; i < _trailInstances.Length; i++)
            {
                if (_trailInstances[i] != null)
                {
                    // Position cible = souris + offset
                    Vector3 targetWorldPos = mouseWorldPos + _trailOffsets[i];

                    // Lerp smooth vers la cible pour créer un mouvement fluide
                    // Plus le lerp est petit, plus le trail "traîne" derrière
                    float smoothSpeed = 0.3f; // Ajuste pour plus/moins de smooth
                    _trailInstances[i].transform.position = Vector3.Lerp(
                        _trailInstances[i].transform.position,
                        targetWorldPos,
                        smoothSpeed
                    );

                    // Contrôler l'opacité selon la vitesse et le threshold
                    float threshold = i < _trailSpeedThresholds.Length ? _trailSpeedThresholds[i] : (i * 0.2f);
                    float targetAlpha = normalizedSpeed >= threshold ? 1f : 0f;

                    // Lerp smooth de l'alpha actuel vers l'alpha cible
                    _trailCurrentAlpha[i] = Mathf.Lerp(_trailCurrentAlpha[i], targetAlpha, _trailFadeSpeed);

                    // Modifier les alpha keys existants (pas d'allocation)
                    for (int k = 0; k < _trailCachedAlphaKeys[i].Length; k++)
                    {
                        // Multiplier l'alpha original par l'alpha courant
                        float originalAlpha = _trailOriginalGradient[i].alphaKeys[k].alpha;
                        _trailCachedAlphaKeys[i][k].alpha = originalAlpha * _trailCurrentAlpha[i];
                    }

                    // Mettre à jour le gradient (réutilise le gradient caché)
                    _trailCachedGradient[i].SetKeys(_trailOriginalGradient[i].colorKeys, _trailCachedAlphaKeys[i]);
                    _trailInstances[i].colorGradient = _trailCachedGradient[i];

                    // Garder le trail toujours actif pour éviter les coupures
                    _trailInstances[i].emitting = true;
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
                              $"Spores: {_sporesInTrigger.Count}\n" +
                              $"Plants: {_plantsInsideWind.Count}";

                UnityEditor.Handles.Label(transform.position + Vector3.up * (_currentRadius + 0.5f), stats, style);
            }
#endif
        }

        #endregion
    }
}
