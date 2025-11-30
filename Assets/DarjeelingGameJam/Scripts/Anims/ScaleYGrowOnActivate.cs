using UnityEngine;

public class ScaleYGrowOnActivate : MonoBehaviour
{
    [Tooltip("Durée de l'animation (en secondes)")]
    public float duration = 0.5f;

    [Tooltip("Lancer automatiquement l'anim quand l'objet est activé")]
    public bool playOnEnable = true;

    private Vector3 _baseScale;
    private Coroutine _scaleRoutine;

    private void Awake()
    {
        _baseScale = transform.localScale;
        // On s'assure que la cible finale est bien Y = 1 par rapport à la base
        _baseScale.y = 1f;
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    /// <summary>
    /// Lance l'animation de scale Y de 0 à 1.
    /// </summary>
    public void Play()
    {
        // On stoppe une éventuelle anim en cours
        if (_scaleRoutine != null)
        {
            StopCoroutine(_scaleRoutine);
        }

        _scaleRoutine = StartCoroutine(ScaleYRoutine());
    }

    private System.Collections.IEnumerator ScaleYRoutine()
    {
        float t = 0f;

        // On part de y = 0, on garde X et Z de base
        Vector3 startScale = new Vector3(_baseScale.x, 0f, _baseScale.z);
        Vector3 endScale = new Vector3(_baseScale.x, 1f, _baseScale.z);

        transform.localScale = startScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;

            float currentY = Mathf.Lerp(0f, 1f, alpha);
            transform.localScale = new Vector3(_baseScale.x, currentY, _baseScale.z);

            yield return null;
        }

        transform.localScale = endScale;
        _scaleRoutine = null;
    }
}
