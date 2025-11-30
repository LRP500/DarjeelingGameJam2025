using UnityEngine;

public class ScaleYGrowOnActivate : MonoBehaviour
{
    [Header("Montée principale")]
    [Tooltip("Durée de la montée initiale (si random désactivé, ou min de la range).")]
    public float duration = 0.5f;

    [Tooltip("Activer un random sur la duration.")]
    public bool randomizeDuration = true;

    [Tooltip("Range de random pour la duration (en secondes).")]
    public float durationMin = 0.4f;
    public float durationMax = 0.7f;

    [Header("Rebond final")]
    [Tooltip("Amplitude du rebond (si random désactivé, ou min de la range).")]
    public float reboundAmount = 0.1f;

    [Tooltip("Activer un random sur l'amplitude du rebond.")]
    public bool randomizeReboundAmount = true;

    [Tooltip("Range de random pour l'amplitude du rebond.")]
    public float reboundAmountMin = 0.05f;
    public float reboundAmountMax = 0.15f;

    [Tooltip("Durée du rebond final (si random désactivé, ou min de la range).")]
    public float reboundDuration = 0.2f;

    [Tooltip("Activer un random sur la durée du rebond.")]
    public bool randomizeReboundDuration = true;

    [Tooltip("Range de random pour la durée du rebond.")]
    public float reboundDurationMin = 0.15f;
    public float reboundDurationMax = 0.3f;

    [Header("Options")]
    [Tooltip("Lancer automatiquement l'anim quand l'objet est activé")]
    public bool playOnEnable = true;

    private Vector3 _baseScale;
    private Coroutine _scaleRoutine;

    private void Awake()
    {
        _baseScale = transform.localScale;
        // On s’assure que la cible finale est bien Y = 1
        _baseScale.y = 1f;
    }

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    /// <summary>
    /// Lance l'animation de scale Y de 0 à 1 avec ease + rebond.
    /// </summary>
    public void Play()
    {
        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);

        // On échantillonne les valeurs pour CE run
        float currentDuration = GetDuration();
        float currentReboundAmount = GetReboundAmount();
        float currentReboundDuration = GetReboundDuration();

        _scaleRoutine = StartCoroutine(ScaleYRoutine(currentDuration, currentReboundAmount, currentReboundDuration));
    }

    float GetDuration()
    {
        if (!randomizeDuration)
            return Mathf.Max(0f, duration);

        float min = Mathf.Max(0f, Mathf.Min(durationMin, durationMax));
        float max = Mathf.Max(0f, Mathf.Max(durationMin, durationMax));
        if (Mathf.Approximately(min, max))
            return min;

        return Random.Range(min, max);
    }

    float GetReboundAmount()
    {
        if (!randomizeReboundAmount)
            return Mathf.Max(0f, reboundAmount);

        float min = Mathf.Max(0f, Mathf.Min(reboundAmountMin, reboundAmountMax));
        float max = Mathf.Max(0f, Mathf.Max(reboundAmountMin, reboundAmountMax));
        if (Mathf.Approximately(min, max))
            return min;

        return Random.Range(min, max);
    }

    float GetReboundDuration()
    {
        if (!randomizeReboundDuration)
            return Mathf.Max(0f, reboundDuration);

        float min = Mathf.Max(0f, Mathf.Min(reboundDurationMin, reboundDurationMax));
        float max = Mathf.Max(0f, Mathf.Max(reboundDurationMin, reboundDurationMax));
        if (Mathf.Approximately(min, max))
            return min;

        return Random.Range(min, max);
    }

    private System.Collections.IEnumerator ScaleYRoutine(float currentDuration, float currentReboundAmount, float currentReboundDuration)
    {
        float t = 0f;

        Vector3 startScale = new Vector3(_baseScale.x, 0f, _baseScale.z);
        Vector3 endScale = new Vector3(_baseScale.x, 1f, _baseScale.z);

        transform.localScale = startScale;

        // --- Phase principale avec ease (SmoothStep) ---
        while (t < currentDuration)
        {
            t += Time.deltaTime;
            float alpha = currentDuration > 0f ? Mathf.Clamp01(t / currentDuration) : 1f;

            // Ease (démarrage en douceur, ralentissement à la fin)
            float eased = Mathf.SmoothStep(0f, 1f, alpha);

            float currentY = Mathf.Lerp(0f, 1f, eased);
            transform.localScale = new Vector3(_baseScale.x, currentY, _baseScale.z);

            yield return null;
        }

        transform.localScale = endScale;

        // --- Rebond final ---
        if (currentReboundAmount > 0f && currentReboundDuration > 0f)
        {
            float reboundT = 0f;

            while (reboundT < currentReboundDuration)
            {
                reboundT += Time.deltaTime;
                float a = currentReboundDuration > 0f ? Mathf.Clamp01(reboundT / currentReboundDuration) : 1f;

                // Damped sine autour de 1 : petit overshoot puis retour doux
                float offset = Mathf.Sin(a * Mathf.PI) * currentReboundAmount * (1f - a);
                float currentY = 1f + offset;

                transform.localScale = new Vector3(_baseScale.x, currentY, _baseScale.z);
                yield return null;
            }

            transform.localScale = endScale;
        }

        _scaleRoutine = null;
    }
}
