using Unity.Cinemachine;
using UnityEngine;

namespace DarjeelingGameJam.Spores
{
    [RequireComponent(typeof(SphereCollider))]
    public class SporeGenerator : MonoBehaviour
    {
        [SerializeField]
        private Spore _spore;

        [SerializeField]
        private float _range = 1f;

        [Vector2AsRange]
        [SerializeField]
        private Vector2 _interval;

        private void Generate()
        {
            
        }
    }
}