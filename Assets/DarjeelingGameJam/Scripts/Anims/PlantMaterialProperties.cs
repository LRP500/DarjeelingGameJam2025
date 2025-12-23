using UnityEngine;

/// <summary>
/// Gère les propriétés shader de la plante via MaterialPropertyBlock
/// pour préserver le GPU batching (évite de créer des material instances)
///
/// Peut être placé sur le parent ou directement sur l'objet avec SpriteRenderer.
/// Le script cherchera automatiquement le SpriteRenderer dans l'objet ou ses enfants.
/// </summary>
public class PlantMaterialProperties : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _propertyBlock;

    // Cache des property IDs pour performance
    private static readonly int RandomSeedID = Shader.PropertyToID("_RandomSeed");
    private static readonly int TintSeedID = Shader.PropertyToID("_TintSeed");
    private static readonly int EnableWindID = Shader.PropertyToID("_EnableWind");
    private static readonly int TintVariationAmountID = Shader.PropertyToID("_TintVariationAmount");

    // Valeurs actuelles (pour que les autres scripts puissent les lire si besoin)
    private float _randomSeed = 0f;
    private float _tintSeed = 0f;
    private float _enableWind = 0f;
    private float _tintVariationAmount = -1f; // -1 = utiliser la valeur du matériau

    private bool _isDirty = false;

    private void Awake()
    {
        // Chercher le SpriteRenderer sur cet objet ou dans les enfants
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer == null)
        {
            Debug.LogError("[PlantMaterialProperties] Aucun SpriteRenderer trouvé sur " + gameObject.name + " ou ses enfants !", this);
            return;
        }

        _propertyBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Définit la seed aléatoire pour la variation de vent (_RandomSeed)
    /// </summary>
    public void SetRandomSeed(float value)
    {
        if (Mathf.Approximately(_randomSeed, value))
            return;

        _randomSeed = value;
        _isDirty = true;
    }

    /// <summary>
    /// Définit la seed aléatoire pour la variation de teinte (_TintSeed)
    /// </summary>
    public void SetTintSeed(float value)
    {
        if (Mathf.Approximately(_tintSeed, value))
            return;

        _tintSeed = value;
        _isDirty = true;
    }

    /// <summary>
    /// Définit l'activation du vent dans le shader (_EnableWind)
    /// Valeur entre 0 (pas de vent) et 1 (vent max)
    /// </summary>
    public void SetEnableWind(float value)
    {
        if (Mathf.Approximately(_enableWind, value))
            return;

        _enableWind = value;
        _isDirty = true;
    }

    /// <summary>
    /// Définit l'intensité de variation de teinte (_TintVariationAmount)
    /// Pour les plantes exceptionnelles
    /// </summary>
    public void SetTintVariationAmount(float value)
    {
        if (Mathf.Approximately(_tintVariationAmount, value))
            return;

        _tintVariationAmount = value;
        _isDirty = true;
    }

    /// <summary>
    /// Applique toutes les propriétés au MaterialPropertyBlock
    /// Appelé automatiquement, mais peut être forcé si besoin
    /// </summary>
    public void Apply()
    {
        if (_spriteRenderer == null || _propertyBlock == null)
            return;

        // Définir toutes les propriétés dans le property block
        _propertyBlock.SetFloat(RandomSeedID, _randomSeed);
        _propertyBlock.SetFloat(TintSeedID, _tintSeed);
        _propertyBlock.SetFloat(EnableWindID, _enableWind);

        // Appliquer la variation de teinte seulement si définie (plantes exceptionnelles)
        if (_tintVariationAmount >= 0f)
        {
            _propertyBlock.SetFloat(TintVariationAmountID, _tintVariationAmount);
        }

        // Appliquer au renderer
        _spriteRenderer.SetPropertyBlock(_propertyBlock);

        _isDirty = false;
    }

    private void LateUpdate()
    {
        // Applique automatiquement si des changements ont été faits ce frame
        if (_isDirty)
        {
            Apply();
        }
    }

    // Getters pour lecture (si besoin)
    public float RandomSeed => _randomSeed;
    public float TintSeed => _tintSeed;
    public float EnableWind => _enableWind;
    public float TintVariationAmount => _tintVariationAmount;
}
