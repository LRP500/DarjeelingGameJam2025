using System.Collections.Generic;
using UnityEngine;

namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// Manager central qui gère une grille spatiale (Spatial Hash Grid) pour les plantes.
    /// Permet au WindTracer de trouver rapidement les plantes proches sans utiliser Physics2D.
    /// </summary>
    public sealed class PlantSpatialGrid : MonoBehaviour
    {
        public static PlantSpatialGrid Instance { get; private set; }

        [Header("Grid Settings")]
        [Tooltip("Taille d'une cellule de la grille (devrait être ≈ rayon du vent)")]
        [SerializeField]
        private float cellSize = 3f;

        // Listes des plantes enregistrées
        private readonly List<Transform> plantTransforms = new List<Transform>(4096);
        private readonly List<IWindAffectable> plantWindComponents = new List<IWindAffectable>(4096);

        // Grille : chaque cellule (key) contient une liste d'indices de plantes
        private readonly Dictionary<long, List<int>> cells = new Dictionary<long, List<int>>(8192);

        private float invCellSize;

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[PlantSpatialGrid] Une instance existe déjà, destruction de cette instance.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            invCellSize = 1f / cellSize;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Enregistre une plante dans la grille spatiale.
        /// Appelé automatiquement par les plantes au OnEnable.
        /// </summary>
        public void RegisterPlant(Transform plantTransform, IWindAffectable windComponent)
        {
            if (plantTransform == null || windComponent == null)
                return;

            int plantId = plantTransforms.Count;
            plantTransforms.Add(plantTransform);
            plantWindComponents.Add(windComponent);

            // Calculer la clé de la cellule
            long cellKey = GetCellKey(plantTransform.position);

            // Ajouter la plante à la cellule
            if (!cells.TryGetValue(cellKey, out var plantList))
            {
                plantList = new List<int>(16);
                cells.Add(cellKey, plantList);
            }
            plantList.Add(plantId);
        }

        /// <summary>
        /// Query la grille pour trouver toutes les plantes candidates dans un rayon donné.
        /// Retourne les indices des plantes dans les cellules autour du centre.
        /// Le filtrage fin (distance exacte) doit être fait après par l'appelant.
        /// </summary>
        public void Query(Vector2 center, float radius, List<int> outPlantIds)
        {
            outPlantIds.Clear();

            // Calculer les limites des cellules à parcourir
            float r = radius;
            int minX = Mathf.FloorToInt((center.x - r) * invCellSize);
            int maxX = Mathf.FloorToInt((center.x + r) * invCellSize);
            int minY = Mathf.FloorToInt((center.y - r) * invCellSize);
            int maxY = Mathf.FloorToInt((center.y + r) * invCellSize);

            // Parcourir toutes les cellules dans cette région
            for (int cy = minY; cy <= maxY; cy++)
            {
                for (int cx = minX; cx <= maxX; cx++)
                {
                    long key = PackCellKey(cx, cy);
                    if (cells.TryGetValue(key, out var plantList))
                    {
                        // Ajout brut, le filtrage précis se fait après au sqrMagnitude
                        outPlantIds.AddRange(plantList);
                    }
                }
            }
        }

        /// <summary>
        /// Obtient la position d'une plante par son ID.
        /// </summary>
        public Vector2 GetPlantPosition(int plantId)
        {
            if (plantId >= 0 && plantId < plantTransforms.Count && plantTransforms[plantId] != null)
            {
                return plantTransforms[plantId].position;
            }
            return default;
        }

        /// <summary>
        /// Obtient le composant IWindAffectable d'une plante par son ID.
        /// </summary>
        public IWindAffectable GetPlantWindComponent(int plantId)
        {
            if (plantId >= 0 && plantId < plantWindComponents.Count)
            {
                return plantWindComponents[plantId];
            }
            return null;
        }

        /// <summary>
        /// Calcule la clé de cellule pour une position donnée.
        /// </summary>
        private long GetCellKey(Vector2 position)
        {
            int cx = Mathf.FloorToInt(position.x * invCellSize);
            int cy = Mathf.FloorToInt(position.y * invCellSize);
            return PackCellKey(cx, cy);
        }

        /// <summary>
        /// Pack les coordonnées de cellule (cx, cy) en un long unique.
        /// </summary>
        private static long PackCellKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField]
        private bool showDebugInfo = false;

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || !Application.isPlaying)
                return;

            // Afficher le nombre de plantes enregistrées
            UnityEditor.Handles.Label(
                transform.position,
                $"PlantSpatialGrid\nPlants: {plantTransforms.Count}\nCells: {cells.Count}",
                new GUIStyle()
                {
                    normal = { textColor = Color.green },
                    fontSize = 12
                }
            );
        }
#endif
    }
}
