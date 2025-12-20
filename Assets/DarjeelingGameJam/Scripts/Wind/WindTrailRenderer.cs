using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Wind
{
    [RequireComponent(typeof(TrailRenderer))]
    public class WindTrailRenderer : MonoBehaviour
    {
        [Header("Trail Settings")]
        [SerializeField] private float _trailTime = 0.5f;
        [SerializeField] private float _trailWidth = 0.3f;
        [SerializeField] private AnimationCurve _widthCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private Gradient _colorGradient;

        [Header("Behavior")]
        [SerializeField] private bool _onlyShowWhenMoving = true;
        [SerializeField] private float _minSpeedToShow = 50f;

        private TrailRenderer _trailRenderer;
        private Camera _mainCamera;
        private MouseVelocityTracker _velocityTracker;

        private void Awake()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
            _mainCamera = Camera.main;
            _velocityTracker = FindObjectOfType<MouseVelocityTracker>();

            InitializeTrailRenderer();
        }

        private void InitializeTrailRenderer()
        {
            _trailRenderer.time = _trailTime;
            _trailRenderer.widthMultiplier = _trailWidth;
            _trailRenderer.widthCurve = _widthCurve;

            // Create default white gradient if none set
            if (_colorGradient == null || _colorGradient.alphaKeys.Length == 0)
            {
                _colorGradient = new Gradient();
                _colorGradient.SetKeys(
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
            }

            _trailRenderer.colorGradient = _colorGradient;
            _trailRenderer.emitting = false;
        }

        private void Update()
        {
            UpdatePosition();
            UpdateEmission();
        }

        private void UpdatePosition()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
            worldPosition.z = 0f;

            transform.position = worldPosition;
        }

        private void UpdateEmission()
        {
            if (_onlyShowWhenMoving && _velocityTracker != null)
            {
                _trailRenderer.emitting = _velocityTracker.Speed >= _minSpeedToShow;
            }
            else
            {
                _trailRenderer.emitting = true;
            }
        }

        public void ClearTrail()
        {
            _trailRenderer.Clear();
        }
    }
}
