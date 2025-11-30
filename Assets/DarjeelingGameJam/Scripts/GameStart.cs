using DarjeelingGameJam.Spores;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DarjeelingGameJam
{
    public class GameStart : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider2D _sporeSpawnArea;

        [SerializeField]
        private Spore _spore;
        
        [MinValue(0)]
        [SerializeField]
        private int _startingSporeCount = 4;
        
        private void Awake()
        {
            for (var i = 0; i < _startingSporeCount; i++)
            {
                var position = RandomPointInBounds(_sporeSpawnArea.bounds);
                var spore = Instantiate(_spore, position, Quaternion.identity);
                spore.Detach();
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
