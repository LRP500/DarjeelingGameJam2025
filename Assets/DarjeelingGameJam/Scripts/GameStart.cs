using System.Collections.Generic;
using DarjeelingGameJam.Spores;
using UnityEngine;

namespace DarjeelingGameJam
{
    public class GameStart : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider2D _sporeSpawnArea;

        [SerializeField]
        private List<Spore> _spores;
        
        private void Awake()
        {
            foreach (var spore in _spores)
            {
                var position = RandomPointInBounds(_sporeSpawnArea.bounds);
                var instance = Instantiate(spore, position, Quaternion.identity);
                instance.Detach();
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
