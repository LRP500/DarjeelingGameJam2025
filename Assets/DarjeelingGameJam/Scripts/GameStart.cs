using System.Collections.Generic;
using DarjeelingGameJam.Spores;
using UnityEngine;

namespace DarjeelingGameJam
{
    [System.Serializable]
    public class SporeSpawnEntry
    {
        [Tooltip("La spore à spawn")]
        public Spore Spore;

        [Tooltip("Nombre minimum de spores de ce type à spawn (0-10)")]
        [Range(0, 10)]
        public int MinSpawnCount = 1;

        [Tooltip("Nombre maximum de spores de ce type à spawn (0-10)")]
        [Range(0, 10)]
        public int MaxSpawnCount = 1;
    }

    public class GameStart : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider2D _sporeSpawnArea;

        [SerializeField]
        private List<SporeSpawnEntry> _spores;

        private void Awake()
        {
            foreach (var entry in _spores)
            {
                // Nombre aléatoire entre Min et Max (inclus)
                int spawnCount = Random.Range(entry.MinSpawnCount, entry.MaxSpawnCount + 1);

                // Spawn X fois la même spore
                for (int i = 0; i < spawnCount; i++)
                {
                    var position = RandomPointInBounds(_sporeSpawnArea.bounds);
                    var instance = Instantiate(entry.Spore, position, Quaternion.identity);
                    instance.Detach();
                }
            }
        }

        private static Vector3 RandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y), 0
            );
        }
    }
}
