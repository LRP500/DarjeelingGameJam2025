using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomPlantWindSeed : MonoBehaviour
{
    [Tooltip("Minimum pour _RandomSeed")]
    public float minSeed = 0f;

    [Tooltip("Maximum pour _RandomSeed")]
    public float maxSeed = 10f;

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            return;

        // .material = instance du material (évite de modifier le shared pour tout le monde)
        var mat = sr.material;
        if (mat == null || !mat.HasProperty("_RandomSeed"))
            return;

        float seed = Random.Range(minSeed, maxSeed);
        mat.SetFloat("_RandomSeed", seed);
    }
}
