using UnityEngine;

/// <summary>
/// Joue un AnimationClip une fois, puis boucle uniquement sur les N dernières frames.
/// Si loopLastFrames <= 0, la dernière frame est figée.
/// </summary>
public class LoopEndOfClip : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    [Tooltip("Le clip qu'on veut lire")]
    public AnimationClip clip;
    [Tooltip("Nom de l'état dans l'Animator qui joue ce clip")]
    public string stateName = "MyState";

    [Header("Boucle")]
    [Tooltip("Nombre de frames de fin à boucler (0 = figer sur la dernière frame)")]
    public int loopLastFrames = 5;

    private float _clipLength;
    private float _loopStartTime;
    private float _loopDuration;
    private float _timer;
    private int _stateHash;
    private bool _hasLoop;

    private void Awake()
    {
        if (clip == null)
        {
            Debug.LogError("[LoopEndOfClip] Pas de clip assigné !");
            enabled = false;
            return;
        }

        _clipLength = clip.length;
        float frameRate = clip.frameRate;

        if (loopLastFrames > 0)
        {
            _loopDuration = loopLastFrames / frameRate;
            _loopDuration = Mathf.Clamp(_loopDuration, 0f, _clipLength);
            _loopStartTime = _clipLength - _loopDuration;
            _hasLoop = _loopDuration > 0f;
        }
        else
        {
            // 0 ou moins = pas de boucle, on reste sur la dernière frame
            _loopDuration = 0f;
            _loopStartTime = _clipLength;
            _hasLoop = false;
        }

        _stateHash = Animator.StringToHash(stateName);
    }

    private void OnEnable()
    {
        _timer = 0f;

        if (animator != null)
        {
            // Le script contrôle le temps
            animator.speed = 0f;

            // On démarre au début
            animator.Play(_stateHash, 0, 0f);
            animator.Update(0f);
        }
    }

    private void Update()
    {
        if (animator == null || clip == null)
            return;

        _timer += Time.deltaTime;

        float currentTime;

        if (!_hasLoop)
        {
            // Pas de loop : on clamp sur la dernière frame
            currentTime = Mathf.Min(_timer, _clipLength);
        }
        else
        {
            if (_timer <= _clipLength)
            {
                // Phase normale (0 -> fin)
                currentTime = _timer;
            }
            else
            {
                // Phase boucle sur les N dernières frames
                float tInsideLoop = (_timer - _clipLength) % _loopDuration;
                currentTime = _loopStartTime + tInsideLoop;
            }
        }

        float normalizedTime = currentTime / _clipLength;

        animator.Play(_stateHash, 0, normalizedTime);
        animator.Update(0f);
    }
}
