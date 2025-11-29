using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LoopEndOfClip : MonoBehaviour
{
    [Header("Loop de fin")]
    [Tooltip("Nombre de frames de fin à boucler (0 = figer sur la dernière frame)")]
    public int loopLastFrames = 5;

    [Header("Vent (logique + shader)")]
    [Tooltip("True = loop 'dans le vent' (saute des frames ET active le vent dans le shader)")]
    public bool windActive = false;

    [Tooltip("Facteur de saut de frames en vent (2 = une frame sur deux)")]
    public int windFrameStep = 2;

    private Animator _animator;
    private AnimationClip _clip;

    private float _clipLength;
    private float _frameDuration;
    private int _totalFrames;

    private int _loopFramesCount;
    private int _loopFirstFrameIndex;
    private float _loopFirstTime;
    private float _lastFrameTime;

    private bool _growthFinished = false;
    private bool _inLoopPhase = false;

    private int _currentLoopFrameIndex = 0;
    private float _frameTimer = 0f;

    private const float LoopSpeed = 1f;

    private SpriteRenderer _spriteRenderer;
    private Material _plantMaterial;
    private static readonly int EnableWindID = Shader.PropertyToID("_EnableWind");

    private int _previousLoopLastFrames;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("[LoopEndOfClip] Aucun Animator trouvé sur ce GameObject.");
            enabled = false;
            return;
        }

        var controller = _animator.runtimeAnimatorController;
        if (controller == null || controller.animationClips == null || controller.animationClips.Length == 0)
        {
            Debug.LogError("[LoopEndOfClip] Pas d'AnimationClip trouvé dans le RuntimeAnimatorController.");
            enabled = false;
            return;
        }

        _clip = controller.animationClips[0];
        _clipLength = _clip.length;

        float frameRate = Mathf.Max(_clip.frameRate, 1f);
        _frameDuration = 1f / frameRate;

        _totalFrames = Mathf.Max(1, Mathf.RoundToInt(_clipLength * frameRate));

        _lastFrameTime = (_totalFrames - 1) * _frameDuration;
        _lastFrameTime = Mathf.Min(_lastFrameTime, _clipLength);

        _previousLoopLastFrames = loopLastFrames;
        RecalculateLoopConfig(false);

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _plantMaterial = _spriteRenderer.material;
        }
        else
        {
            Debug.LogWarning("[LoopEndOfClip] Aucun SpriteRenderer trouvé, le shader PlantWind ne sera pas piloté.");
        }
    }

    private void OnEnable()
    {
        _growthFinished = false;
        _inLoopPhase = false;
        _currentLoopFrameIndex = 0;
        _frameTimer = 0f;

        if (_animator != null)
        {
            _animator.speed = 1f;
        }

        ApplyWindToMaterial();
    }

    private void Update()
    {
        if (_animator == null || _clip == null)
            return;

        // Si tu changes loopLastFrames en Play → on recalcule la loop
        if (loopLastFrames != _previousLoopLastFrames)
        {
            _previousLoopLastFrames = loopLastFrames;
            RecalculateLoopConfig(true);
        }

        ApplyWindToMaterial();

        if (!_inLoopPhase)
        {
            CheckGrowthFinishedAndEnterLoop();
        }
        else
        {
            UpdateLoop();
        }
    }

    private void RecalculateLoopConfig(bool snapToLoopStartIfLooping)
    {
        if (loopLastFrames > 0)
        {
            _loopFramesCount = Mathf.Clamp(loopLastFrames, 1, _totalFrames);
            _loopFirstFrameIndex = Mathf.Max(0, _totalFrames - _loopFramesCount);
            _loopFirstTime = _loopFirstFrameIndex * _frameDuration;
            _loopFirstTime = Mathf.Min(_loopFirstTime, _lastFrameTime);
        }
        else
        {
            _loopFramesCount = 0;
            _loopFirstFrameIndex = _totalFrames - 1;
            _loopFirstTime = _lastFrameTime;
        }

        if (snapToLoopStartIfLooping && _inLoopPhase && _animator != null)
        {
            _frameTimer = 0f;
            _currentLoopFrameIndex = 0;

            float startTime = _loopFramesCount > 0 ? _loopFirstTime : _lastFrameTime;
            float norm = (_clipLength > 0f) ? startTime / _clipLength : 0f;
            _animator.Play(0, 0, norm);
            _animator.Update(0f);
        }
    }

    private void ApplyWindToMaterial()
    {
        if (_plantMaterial == null)
            return;

        _plantMaterial.SetFloat(EnableWindID, windActive ? 1f : 0f);
    }

    private void CheckGrowthFinishedAndEnterLoop()
    {
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (_growthFinished)
            return;

        if (stateInfo.normalizedTime >= 1f)
        {
            _growthFinished = true;
            EnterLoopPhase();
        }
    }

    private void EnterLoopPhase()
    {
        _inLoopPhase = true;
        _frameTimer = 0f;
        _currentLoopFrameIndex = 0;

        _animator.speed = 0f;

        float startTime = _loopFramesCount > 0 ? _loopFirstTime : _lastFrameTime;

        float norm = (_clipLength > 0f) ? startTime / _clipLength : 0f;
        _animator.Play(0, 0, norm);
        _animator.Update(0f);
    }

    private void UpdateLoop()
    {
        if (_loopFramesCount <= 0)
        {
            float normLast = (_clipLength > 0f) ? _lastFrameTime / _clipLength : 1f;
            _animator.Play(0, 0, normLast);
            _animator.Update(0f);
            return;
        }

        float stepInterval = _frameDuration / Mathf.Max(LoopSpeed, 0.01f);

        _frameTimer += Time.deltaTime;
        if (_frameTimer >= stepInterval)
        {
            _frameTimer -= stepInterval;

            int step = 1;
            if (windActive)
            {
                step = Mathf.Max(1, windFrameStep);
            }

            _currentLoopFrameIndex = (_currentLoopFrameIndex + step) % _loopFramesCount;
        }

        int clipFrameIndex = _loopFirstFrameIndex + _currentLoopFrameIndex;
        clipFrameIndex = Mathf.Clamp(clipFrameIndex, 0, _totalFrames - 1);

        float timeOnClip = clipFrameIndex * _frameDuration;
        timeOnClip = Mathf.Clamp(timeOnClip, 0f, _lastFrameTime);

        float normalized = (_clipLength > 0f) ? timeOnClip / _clipLength : 0f;

        _animator.Play(0, 0, normalized);
        _animator.Update(0f);
    }
}
