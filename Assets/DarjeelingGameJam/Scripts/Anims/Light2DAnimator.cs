using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Anime l'intensité et la couleur d'une Light2D dans le temps
/// entre une valeur de base et une valeur finale.
/// </summary>
[RequireComponent(typeof(Light2D))]
public class SimpleLight2DAnimator : MonoBehaviour
{
    [Header("Référence")]
    public Light2D targetLight;

    [Header("Timing")]
    [Tooltip("Durée de l'animation (aller simple) en secondes.")]
    public float duration = 1f;

    [Tooltip("Démarrer automatiquement au OnEnable.")]
    public bool playOnEnable = true;

    [Tooltip("Boucler en ping-pong (base -> final -> base -> ...).")]
    public bool pingPong = true;

    [Header("Intensité")]
    [Tooltip("Intensité au début de l'animation.")]
    public float startIntensity = 1f;

    [Tooltip("Intensité à la fin de l'animation.")]
    public float endIntensity = 2f;

    [Header("Couleur")]
    [Tooltip("Couleur au début de l'animation.")]
    public Color startColor = Color.white;

    [Tooltip("Couleur à la fin de l'animation.")]
    public Color endColor = Color.yellow;

    private float _time;
    private bool _isPlaying;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light2D>();
    }

    private void OnEnable()
    {
        _time = 0f;
        _isPlaying = playOnEnable;

        if (targetLight != null)
        {
            targetLight.intensity = startIntensity;
            targetLight.color = startColor;
        }
    }

    private void Update()
    {
        if (!_isPlaying || targetLight == null || duration <= 0f)
            return;

        _time += Time.deltaTime;

        float t = _time / duration;

        if (pingPong)
        {
            // Ping-pong 0..1..0..1...
            t = Mathf.PingPong(_time / duration, 1f);
        }
        else
        {
            // Clamp simple 0..1
            t = Mathf.Clamp01(t);
            if (t >= 1f)
                _isPlaying = false;
        }

        // Interpolation intensité et couleur
        float intensity = Mathf.Lerp(startIntensity, endIntensity, t);
        Color color = Color.Lerp(startColor, endColor, t);

        targetLight.intensity = intensity;
        targetLight.color = color;
    }

    /// <summary>
    /// Relance l'animation depuis le début.
    /// </summary>
    public void PlayFromStart()
    {
        _time = 0f;
        _isPlaying = true;

        if (targetLight != null)
        {
            targetLight.intensity = startIntensity;
            targetLight.color = startColor;
        }
    }

    /// <summary>
    /// Met en pause / reprend.
    /// </summary>
    public void SetPlaying(bool play)
    {
        _isPlaying = play;
    }
}
