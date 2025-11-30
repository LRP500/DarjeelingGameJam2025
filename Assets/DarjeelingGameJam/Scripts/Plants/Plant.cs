using Unity.Cinemachine;
using UnityEngine;

namespace DarjeelingGameJam.Plants
{
    public class Plant : MonoBehaviour
    {
        [Vector2AsRange]
        [SerializeField]
        private Vector2 _scaleMinMax = new(0.7f, 1.3f);
        
        private void Awake()
        {
            var scale = Random.Range(_scaleMinMax.x, _scaleMinMax.y);
            transform.localScale = new Vector3(scale, scale, 0);
        }
    }
}