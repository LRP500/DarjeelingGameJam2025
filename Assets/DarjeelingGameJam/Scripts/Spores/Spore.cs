using Sirenix.OdinInspector;
using UnityEngine;

namespace DarjeelingGameJam.Spores
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Spore : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        [Button]
        private void Attach()
        {
            _rigidbody.simulated = false;
        }
        
        [Button]
        private void Detach()
        {
            _rigidbody.simulated = true;
        }
    }
}