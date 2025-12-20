using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Script Editor pour ajouter automatiquement PlantMaterialProperties à tous les prefabs de plantes
/// Menu : Tools → Add PlantMaterialProperties to All Plant Prefabs
///
/// SIMPLIFIÉ : Ajoute le composant sur le GameObject parent (celui avec Plant.cs)
/// Le composant cherchera automatiquement le SpriteRenderer dans les enfants.
/// </summary>
public class AddPlantMaterialPropertiesToPrefabs : EditorWindow
{
    [MenuItem("Tools/Plant Optimization/Add PlantMaterialProperties to All Plant Prefabs")]
    static void AddToAllPrefabs()
    {
        // Chercher tous les prefabs dans le dossier Plants (prefabs parents)
        string prefabFolder = "Assets/DarjeelingGameJam/Prefabs/Plants";

        if (!Directory.Exists(prefabFolder))
        {
            Debug.LogError($"Le dossier {prefabFolder} n'existe pas !");
            return;
        }

        string[] prefabPaths = Directory.GetFiles(
            prefabFolder,
            "*.prefab",
            SearchOption.AllDirectories
        );

        if (prefabPaths.Length == 0)
        {
            Debug.LogWarning($"Aucun prefab trouvé dans {prefabFolder}");
            return;
        }

        int updated = 0;
        int skipped = 0;
        int errors = 0;

        Debug.Log($"<color=cyan>Début du traitement de {prefabPaths.Length} prefabs...</color>");

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"Impossible de charger le prefab : {path}");
                errors++;
                continue;
            }

            // Vérifier si le prefab a déjà PlantMaterialProperties
            if (prefab.GetComponent<PlantMaterialProperties>() != null)
            {
                Debug.Log($"  ⊘ Skipped {prefab.name} - a déjà PlantMaterialProperties");
                skipped++;
                continue;
            }

            // Vérifier qu'il a bien un SpriteRenderer dans lui ou ses enfants
            if (prefab.GetComponentInChildren<SpriteRenderer>() == null)
            {
                Debug.LogWarning($"  ⚠ {prefab.name} n'a pas de SpriteRenderer (dans lui ou ses enfants), skipped");
                skipped++;
                continue;
            }

            try
            {
                // Charger le prefab en mode édition
                string assetPath = AssetDatabase.GetAssetPath(prefab);
                GameObject instance = PrefabUtility.LoadPrefabContents(assetPath);

                // Ajouter le composant PlantMaterialProperties sur le ROOT (parent)
                instance.AddComponent<PlantMaterialProperties>();

                // Sauvegarder le prefab
                PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
                PrefabUtility.UnloadPrefabContents(instance);

                Debug.Log($"  ✓ Updated {prefab.name} (composant ajouté sur le parent)");
                updated++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"  ✗ Erreur sur {prefab.name}: {e.Message}");
                errors++;
            }
        }

        // Forcer la sauvegarde et le refresh
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Résumé
        Debug.Log($"\n<color=green>═══════════════════════════════════════════════════════</color>");
        Debug.Log($"<color=green>TRAITEMENT TERMINÉ :</color>");
        Debug.Log($"  <color=lime>✓ {updated} prefabs mis à jour (PlantMaterialProperties ajouté sur le parent)</color>");
        Debug.Log($"  <color=yellow>⊘ {skipped} prefabs skipped (déjà à jour ou pas de SpriteRenderer)</color>");
        Debug.Log($"  <color=red>✗ {errors} erreurs</color>");
        Debug.Log($"<color=green>═══════════════════════════════════════════════════════</color>\n");

        if (updated > 0)
        {
            EditorUtility.DisplayDialog(
                "Optimisation GPU Batching",
                $"Succès !\n\n" +
                $"✓ {updated} prefabs mis à jour avec PlantMaterialProperties\n" +
                $"   (composant ajouté sur le GameObject parent)\n\n" +
                $"⊘ {skipped} prefabs déjà à jour\n" +
                $"✗ {errors} erreurs\n\n" +
                $"Le GPU batching est maintenant activé pour vos plantes !\n" +
                $"Vérifiez les draw calls dans Window → Analysis → Frame Debugger",
                "OK"
            );
        }
        else if (skipped > 0 && updated == 0)
        {
            EditorUtility.DisplayDialog(
                "Prefabs Déjà À Jour",
                $"Tous les {skipped} prefabs ont déjà PlantMaterialProperties.\n\n" +
                $"Rien à faire !",
                "OK"
            );
        }
    }

    [MenuItem("Tools/Plant Optimization/Verify All Plant Prefabs Have PlantMaterialProperties")]
    static void VerifyAllPrefabs()
    {
        string prefabFolder = "Assets/DarjeelingGameJam/Prefabs/Plants";

        if (!Directory.Exists(prefabFolder))
        {
            Debug.LogError($"Le dossier {prefabFolder} n'existe pas !");
            return;
        }

        string[] prefabPaths = Directory.GetFiles(
            prefabFolder,
            "*.prefab",
            SearchOption.AllDirectories
        );

        int withComponent = 0;
        int withoutComponent = 0;

        Debug.Log($"<color=cyan>Vérification de {prefabPaths.Length} prefabs...</color>");

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            if (prefab.GetComponent<PlantMaterialProperties>() != null)
            {
                Debug.Log($"  <color=green>✓</color> {prefab.name} - OK (PlantMaterialProperties sur le parent)");
                withComponent++;
            }
            else
            {
                Debug.LogWarning($"  <color=red>✗</color> {prefab.name} - PlantMaterialProperties MANQUANT sur le parent !");
                withoutComponent++;
            }
        }

        Debug.Log($"\n<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log($"<color=cyan>RÉSULTAT VÉRIFICATION :</color>");
        Debug.Log($"  <color=green>✓ {withComponent} prefabs OK</color>");
        Debug.Log($"  <color=red>✗ {withoutComponent} prefabs manquants</color>");
        Debug.Log($"<color=cyan>═══════════════════════════════════════════════════════</color>\n");

        if (withoutComponent > 0)
        {
            if (EditorUtility.DisplayDialog(
                "Prefabs Incomplets Détectés",
                $"{withoutComponent} prefab(s) n'ont pas PlantMaterialProperties sur le parent.\n\n" +
                $"Voulez-vous les mettre à jour automatiquement ?",
                "Oui, mettre à jour",
                "Non"))
            {
                AddToAllPrefabs();
            }
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Vérification Complète",
                $"Tous les {withComponent} prefabs de plantes ont PlantMaterialProperties !\n\n" +
                $"Le GPU batching devrait fonctionner correctement.",
                "Excellent !"
            );
        }
    }

    [MenuItem("Tools/Plant Optimization/Remove PlantMaterialProperties from All Prefabs")]
    static void RemoveFromAllPrefabs()
    {
        if (!EditorUtility.DisplayDialog(
            "Confirmation",
            "Voulez-vous vraiment SUPPRIMER PlantMaterialProperties de tous les prefabs ?\n\n" +
            "Cette action est utile si vous voulez recommencer l'optimisation.",
            "Oui, supprimer",
            "Annuler"))
        {
            return;
        }

        string prefabFolder = "Assets/DarjeelingGameJam/Prefabs/Plants";
        string[] prefabPaths = Directory.GetFiles(prefabFolder, "*.prefab", SearchOption.AllDirectories);

        int removed = 0;

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            if (prefab.GetComponent<PlantMaterialProperties>() == null)
                continue;

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject instance = PrefabUtility.LoadPrefabContents(assetPath);

            PlantMaterialProperties comp = instance.GetComponent<PlantMaterialProperties>();
            if (comp != null)
            {
                DestroyImmediate(comp);
                PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
                removed++;
            }

            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=yellow>PlantMaterialProperties supprimé de {removed} prefabs</color>");
        EditorUtility.DisplayDialog("Suppression Terminée", $"PlantMaterialProperties supprimé de {removed} prefabs.", "OK");
    }
}
