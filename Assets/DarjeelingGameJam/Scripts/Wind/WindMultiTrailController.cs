using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// Contrôle plusieurs trails visuels qui s'activent progressivement selon la vitesse de la souris.
    /// Quand la souris bouge vite, plus de trails apparaissent pour un effet visuel intense.
    /// </summary>
    public class WindMultiTrailController : MonoBehaviour
    {
        [Header("Trail Pool")]
        [Tooltip("Nombre maximum de trails (ex: 5)")]
        [SerializeField]
        private int _maxTrails = 5;

        [Tooltip("Prefab de trail à utiliser (avec TrailRenderer)")]
        [SerializeField]
        private GameObject _trailPrefab;

        [Header("Speed Thresholds")]
        [Tooltip("Vitesse normalisée minimale pour activer les trails (0-1)")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _minSpeedToShow = 0.1f;

        [Tooltip("Vitesse normalisée pour activer tous les trails (0-1)")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _maxSpeedForAllTrails = 0.8f;

        [Header("Trail Positioning")]
        [Tooltip("Distance perpendiculaire entre les trails (en world units)")]
        [SerializeField]
        private float _trailSpacing = 0.2f;

        [Tooltip("Variation aléatoire de position des trails")]
        [SerializeField]
        private float _randomOffset = 0.1f;

        [Header("Trail Appearance")]
        [Tooltip("Largeur du trail principal")]
        [SerializeField]
        private float _mainTrailWidth = 0.3f;

        [Tooltip("Largeur des trails secondaires (multiplicateur)")]
        [SerializeField]
        private float _secondaryTrailWidthMultiplier = 0.7f;

        [Tooltip("Temps de vie du trail")]
        [SerializeField]
        private float _trailTime = 0.5f;

        [Tooltip("Gradient de couleur du trail")]
        [SerializeField]
        private Gradient _trailGradient;

        private Camera _mainCamera;
        private MouseVelocityTracker _velocityTracker;
        private TrailRenderer[] _trails;
        private Vector3[] _trailOffsets;
        private int _activeTrailCount = 0;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _velocityTracker = FindObjectOfType<MouseVelocityTracker>();

            if (_velocityTracker == null)
            {
                Debug.LogError("[WindMultiTrailController] MouseVelocityTracker introuvable !", this);
                enabled = false;
                return;
            }

            InitializeTrailPool();
        }

        private void InitializeTrailPool()
        {
            _trails = new TrailRenderer[_maxTrails];
            _trailOffsets = new Vector3[_maxTrails];

            for (int i = 0; i < _maxTrails; i++)
            {
                GameObject trailObj;

                if (_trailPrefab != null)
                {
                    // Utiliser le prefab fourni
                    trailObj = Instantiate(_trailPrefab, transform);
                }
                else
                {
                    // Créer un trail par défaut
                    trailObj = new GameObject($"Trail_{i}");
                    trailObj.transform.SetParent(transform);
                    trailObj.AddComponent<TrailRenderer>();
                }

                _trails[i] = trailObj.GetComponent<TrailRenderer>();

                if (_trails[i] == null)
                {
                    Debug.LogError($"[WindMultiTrailController] Le trail {i} n'a pas de TrailRenderer !", this);
                    continue;
                }

                // Configurer le trail
                ConfigureTrail(_trails[i], i);

                // Calculer l'offset pour ce trail
                CalculateTrailOffset(i);

                // Désactiver par défaut
                _trails[i].emitting = false;
            }
        }

        private void ConfigureTrail(TrailRenderer trail, int index)
        {
            trail.time = _trailTime;

            // Le trail principal (0) est plus large
            float widthMultiplier = (index == 0) ? 1f : _secondaryTrailWidthMultiplier;
            trail.widthMultiplier = _mainTrailWidth * widthMultiplier;

            // Courbe de largeur (commence large, finit fin)
            trail.widthCurve = AnimationCurve.Linear(0, 1, 1, 0);

            // Gradient de couleur
            if (_trailGradient != null && _trailGradient.alphaKeys.Length > 0)
            {
                trail.colorGradient = _trailGradient;
            }
            else
            {
                // Gradient par défaut : blanc qui fade
                Gradient defaultGradient = new Gradient();
                defaultGradient.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(Color.white, 1f)
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                trail.colorGradient = defaultGradient;
            }

            // Désactiver les ombres pour les trails
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
        }

        private void CalculateTrailOffset(int index)
        {
            if (index == 0)
            {
                // Trail principal au centre
                _trailOffsets[index] = Vector3.zero;
            }
            else
            {
                // Trails secondaires alternent de chaque côté
                // Index 1, 3, 5... vont à droite
                // Index 2, 4, 6... vont à gauche
                int side = (index % 2 == 1) ? 1 : -1;
                int layer = (index + 1) / 2;

                // Offset perpendiculaire
                float perpOffset = side * layer * _trailSpacing;

                // Petit offset aléatoire pour varier
                float randomX = Random.Range(-_randomOffset, _randomOffset);
                float randomY = Random.Range(-_randomOffset, _randomOffset);

                _trailOffsets[index] = new Vector3(randomX, perpOffset + randomY, 0f);
            }
        }

        private void Update()
        {
            if (_velocityTracker == null) return;

            UpdateActiveTrailCount();
            UpdateTrailPositions();
        }

        private void UpdateActiveTrailCount()
        {
            float normalizedSpeed = _velocityTracker.NormalizedSpeed;

            // Calculer combien de trails doivent être actifs
            int targetTrailCount = 0;

            if (normalizedSpeed >= _minSpeedToShow)
            {
                // Lerp entre 1 trail et maxTrails selon la vitesse
                float speedRange = _maxSpeedForAllTrails - _minSpeedToShow;
                float speedInRange = Mathf.Clamp01((normalizedSpeed - _minSpeedToShow) / speedRange);

                // Au minimum 1 trail si on bouge, maximum _maxTrails
                targetTrailCount = Mathf.RoundToInt(Mathf.Lerp(1, _maxTrails, speedInRange));
            }

            // Activer/désactiver les trails progressivement
            if (targetTrailCount != _activeTrailCount)
            {
                _activeTrailCount = targetTrailCount;

                for (int i = 0; i < _maxTrails; i++)
                {
                    if (_trails[i] == null) continue;

                    bool shouldEmit = i < _activeTrailCount;

                    if (shouldEmit && !_trails[i].emitting)
                    {
                        _trails[i].emitting = true;
                    }
                    else if (!shouldEmit && _trails[i].emitting)
                    {
                        _trails[i].emitting = false;
                    }
                }
            }
        }

        private void UpdateTrailPositions()
        {
            // Position de base = position de la souris
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 baseWorldPosition = _mainCamera.ScreenToWorldPoint(
                new Vector3(mousePosition.x, mousePosition.y, 10f)
            );
            baseWorldPosition.z = 0f;

            // Direction de mouvement pour orienter les offsets
            Vector2 direction = _velocityTracker.GetDirection();
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

            // Mettre à jour la position de chaque trail actif
            for (int i = 0; i < _activeTrailCount; i++)
            {
                if (_trails[i] == null) continue;

                // Calculer la position avec offset
                Vector3 offset = _trailOffsets[i];

                // Appliquer l'offset perpendiculaire dans la direction du mouvement
                Vector3 worldOffset = perpendicular * offset.y + new Vector3(offset.x, 0, 0);

                _trails[i].transform.position = baseWorldPosition + worldOffset;
            }
        }

        /// <summary>
        /// Efface tous les trails (utile pour reset)
        /// </summary>
        public void ClearAllTrails()
        {
            foreach (var trail in _trails)
            {
                if (trail != null)
                {
                    trail.Clear();
                }
            }
        }

        private void OnValidate()
        {
            // S'assurer que les seuils sont logiques
            if (_maxSpeedForAllTrails < _minSpeedToShow)
            {
                _maxSpeedForAllTrails = _minSpeedToShow;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || _trails == null) return;

            // Dessiner les positions des trails actifs
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _activeTrailCount; i++)
            {
                if (_trails[i] != null)
                {
                    Gizmos.DrawWireSphere(_trails[i].transform.position, 0.1f);
                }
            }
        }
#endif
    }
}
