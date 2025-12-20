using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Script de diagnostic pour identifier ce qui casse le GPU batching
/// Menu : Tools → Plant Optimization → Diagnose Batching Issues
///
/// Affiche tous les renderers actifs groupés par material/shader pour identifier
/// les sources de draw calls
/// </summary>
public class BatchingDiagnostic : EditorWindow
{
    [MenuItem("Tools/Plant Optimization/Diagnose Batching Issues (Play Mode Only)")]
    static void DiagnoseBatching()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Play Mode Requis",
                "Ce diagnostic doit être lancé en PLAY MODE.\n\n" +
                "1. Lancez le jeu (Play)\n" +
                "2. Faites apparaître beaucoup de plantes\n" +
                "3. Relancez ce diagnostic",
                "OK"
            );
            return;
        }

        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan>DIAGNOSTIC DES BATCHES - ANALYSE EN COURS...</color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════════════</color>\n");

        // Récupérer tous les renderers actifs dans la scène
        var allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var spriteRenderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);

        Debug.Log($"<color=yellow>Total Renderers actifs : {allRenderers.Length}</color>");
        Debug.Log($"<color=yellow>Total SpriteRenderers actifs : {spriteRenderers.Length}</color>\n");

        // Grouper par material
        var byMaterial = new Dictionary<Material, List<Renderer>>();
        var byShader = new Dictionary<Shader, List<Renderer>>();
        var byTag = new Dictionary<string, List<Renderer>>();

        foreach (var renderer in allRenderers)
        {
            if (renderer.sharedMaterial != null)
            {
                // Par material
                if (!byMaterial.ContainsKey(renderer.sharedMaterial))
                    byMaterial[renderer.sharedMaterial] = new List<Renderer>();
                byMaterial[renderer.sharedMaterial].Add(renderer);

                // Par shader
                if (renderer.sharedMaterial.shader != null)
                {
                    if (!byShader.ContainsKey(renderer.sharedMaterial.shader))
                        byShader[renderer.sharedMaterial.shader] = new List<Renderer>();
                    byShader[renderer.sharedMaterial.shader].Add(renderer);
                }
            }

            // Par tag
            string tag = string.IsNullOrEmpty(renderer.gameObject.tag) ? "Untagged" : renderer.gameObject.tag;
            if (!byTag.ContainsKey(tag))
                byTag[tag] = new List<Renderer>();
            byTag[tag].Add(renderer);
        }

        // Afficher par shader (le plus important pour batching)
        Debug.Log("<color=lime>═══ RENDERERS PAR SHADER ═══</color>");
        foreach (var kvp in byShader.OrderByDescending(x => x.Value.Count))
        {
            Debug.Log($"  <color=white>{kvp.Key.name}</color> : <color=yellow>{kvp.Value.Count} renderers</color>");
        }
        Debug.Log("");

        // Afficher par material
        Debug.Log("<color=lime>═══ RENDERERS PAR MATERIAL (Top 10) ═══</color>");
        int count = 0;
        foreach (var kvp in byMaterial.OrderByDescending(x => x.Value.Count))
        {
            if (count++ >= 10) break;
            Debug.Log($"  <color=white>{kvp.Key.name}</color> : <color=yellow>{kvp.Value.Count} renderers</color>");
        }
        Debug.Log("");

        // Afficher par tag (pour identifier les types d'objets)
        Debug.Log("<color=lime>═══ RENDERERS PAR TAG ═══</color>");
        foreach (var kvp in byTag.OrderByDescending(x => x.Value.Count))
        {
            Debug.Log($"  <color=white>{kvp.Key}</color> : <color=yellow>{kvp.Value.Count} renderers</color>");
        }
        Debug.Log("");

        // Analyse spécifique : Plantes
        int plantsWithMaterialProps = 0;
        int plantsWithoutMaterialProps = 0;
        var plants = Object.FindObjectsByType<PlantMaterialProperties>(FindObjectsSortMode.None);

        Debug.Log("<color=lime>═══ ANALYSE DES PLANTES ═══</color>");
        Debug.Log($"  Plantes avec PlantMaterialProperties : <color=green>{plants.Length}</color>");

        // Compter les plantes sans le composant (celles avec LoopEndOfClip mais pas PlantMaterialProperties)
        var loopEndOfClips = Object.FindObjectsByType<LoopEndOfClip>(FindObjectsSortMode.None);
        foreach (var loop in loopEndOfClips)
        {
            var plantMatProps = loop.GetComponentInParent<PlantMaterialProperties>();
            if (plantMatProps != null)
                plantsWithMaterialProps++;
            else
                plantsWithoutMaterialProps++;
        }

        if (plantsWithoutMaterialProps > 0)
        {
            Debug.LogWarning($"  ⚠ Plantes SANS PlantMaterialProperties : <color=red>{plantsWithoutMaterialProps}</color>");
            Debug.LogWarning($"    → Ces plantes cassent le batching !");
        }
        else
        {
            Debug.Log($"  ✓ Toutes les plantes ont PlantMaterialProperties");
        }
        Debug.Log("");

        // Analyse spécifique : Spores
        var spores = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Where(sr => sr.gameObject.name.ToLower().Contains("spore"))
            .ToList();

        Debug.Log("<color=lime>═══ ANALYSE DES SPORES ═══</color>");
        Debug.Log($"  Spores actives (approximatif) : <color=yellow>{spores.Count}</color>");

        if (spores.Count > 0)
        {
            var sporeMaterials = spores.Select(s => s.sharedMaterial).Distinct().ToList();
            Debug.Log($"  Materials de spores différents : <color=yellow>{sporeMaterials.Count}</color>");

            if (sporeMaterials.Count > 1)
            {
                Debug.LogWarning($"  ⚠ Plusieurs materials différents pour les spores !");
                foreach (var mat in sporeMaterials)
                {
                    int countForMat = spores.Count(s => s.sharedMaterial == mat);
                    Debug.LogWarning($"    → {mat?.name ?? "null"} : {countForMat} spores");
                }
            }
        }
        Debug.Log("");

        // Analyse : Particules
        var particleSystems = Object.FindObjectsByType<ParticleSystemRenderer>(FindObjectsSortMode.None);
        Debug.Log("<color=lime>═══ ANALYSE DES PARTICULES ═══</color>");
        Debug.Log($"  ParticleSystems actifs : <color=yellow>{particleSystems.Length}</color>");

        if (particleSystems.Length > 10)
        {
            Debug.LogWarning($"  ⚠ Beaucoup de particle systems actifs ! Chacun crée 1+ draw call");
        }
        Debug.Log("");

        // Détection de material instances (le problème principal)
        Debug.Log("<color=lime>═══ DÉTECTION MATERIAL INSTANCES ═══</color>");
        int materialInstances = 0;
        var materialNames = new HashSet<string>();

        foreach (var renderer in allRenderers)
        {
            if (renderer.sharedMaterial != null)
            {
                // Si le nom du material contient "(Instance)", c'est une instance
                if (renderer.sharedMaterial.name.Contains("Instance"))
                {
                    materialInstances++;
                    materialNames.Add(renderer.sharedMaterial.name);
                }
            }
        }

        if (materialInstances > 0)
        {
            Debug.LogError($"  ❌ PROBLÈME DÉTECTÉ : {materialInstances} renderers utilisent des material instances !");
            Debug.LogError($"     → Cela CASSE le GPU batching complètement");
            Debug.LogError($"     Materials concernés :");
            foreach (var name in materialNames)
            {
                Debug.LogError($"       • {name}");
            }
        }
        else
        {
            Debug.Log($"  ✓ Aucune material instance détectée (bon signe !)");
        }
        Debug.Log("");

        // Estimation des batches attendus
        Debug.Log("<color=lime>═══ ESTIMATION DES BATCHES ═══</color>");
        Debug.Log($"  Shaders différents : <color=yellow>{byShader.Count}</color>");
        Debug.Log($"  Materials différents : <color=yellow>{byMaterial.Count}</color>");
        Debug.Log($"  ");
        Debug.Log($"  <color=white>Batches minimum attendus :</color>");
        Debug.Log($"    • Si SRP Batcher activé : <color=green>~{byShader.Count}-{byShader.Count * 2}</color>");
        Debug.Log($"    • Si batching dynamique : <color=green>~{byMaterial.Count}</color>");
        Debug.Log($"    • Sans optimisation : <color=red>~{allRenderers.Length}</color>");
        Debug.Log("");

        // Recommandations
        Debug.Log("<color=lime>═══ RECOMMANDATIONS ═══</color>");

        if (materialInstances > 0)
        {
            Debug.LogWarning("  1. <color=red>CRITIQUE</color> : Éliminer les material instances");
            Debug.LogWarning("     → Utiliser MaterialPropertyBlock pour toutes les variations");
        }

        if (spores.Count > 20)
        {
            Debug.LogWarning($"  2. <color=yellow>IMPORTANT</color> : {spores.Count} spores actives");
            Debug.LogWarning("     → Implémenter object pooling pour les spores");
            Debug.LogWarning("     → Limiter le max à 20-30 spores simultanées");
        }

        if (particleSystems.Length > 10)
        {
            Debug.LogWarning($"  3. <color=yellow>AMÉLIORATION</color> : {particleSystems.Length} particle systems");
            Debug.LogWarning("     → Pooler et recycler les particules de vent");
        }

        if (byMaterial.Count > 10)
        {
            Debug.LogWarning($"  4. <color=yellow>AMÉLIORATION</color> : {byMaterial.Count} materials différents");
            Debug.LogWarning("     → Essayer de consolider certains materials si possible");
        }

        Debug.Log("");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan>DIAGNOSTIC TERMINÉ - Consultez la console pour les détails</color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════════════</color>");

        // Dialog final
        string summary = $"Renderers actifs: {allRenderers.Length}\n" +
                        $"Shaders différents: {byShader.Count}\n" +
                        $"Materials différents: {byMaterial.Count}\n" +
                        $"Spores actives: {spores.Count}\n" +
                        $"Particle Systems: {particleSystems.Length}\n\n";

        if (materialInstances > 0)
            summary += $"❌ {materialInstances} material instances détectées !\n";
        else
            summary += "✓ Pas de material instances\n";

        EditorUtility.DisplayDialog(
            "Diagnostic Batching",
            summary + "\nConsultez la console pour les détails complets.",
            "OK"
        );
    }
}
