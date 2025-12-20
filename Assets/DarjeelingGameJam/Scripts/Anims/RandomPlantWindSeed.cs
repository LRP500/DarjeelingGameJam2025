using UnityEngine;

/// <summary>
/// Définit une seed aléatoire pour varier l'animation de vent de chaque plante.
/// Cherche PlantMaterialProperties sur cet objet ou dans le parent.
/// </summary>
public class RandomPlantWindSeed : MonoBehaviour
{
    [Tooltip("Minimum pour _RandomSeed")]
    public float minSeed = 0f;

    [Tooltip("Maximum pour _RandomSeed")]
    public float maxSeed = 10f;

    void Awake()
    {
        // Chercher PlantMaterialProperties sur cet objet ou dans le parent
        var materialProps = GetComponentInParent<PlantMaterialProperties>();
        if (materialProps == null)
        {
            Debug.LogWarning("[RandomPlantWindSeed] PlantMaterialProperties manquant sur " + gameObject.name + " ou son parent", this);
            return;
        }

        float seed = Random.Range(minSeed, maxSeed);
        materialProps.SetRandomSeed(seed);
        materialProps.Apply();
    }
}
