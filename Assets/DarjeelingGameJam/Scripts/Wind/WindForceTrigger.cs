using UnityEngine;

namespace DarjeelingGameJam.Wind
{
    public class WindForceTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.name);
        }
    }
}