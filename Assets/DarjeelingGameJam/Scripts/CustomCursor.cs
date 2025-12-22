using UnityEngine;

namespace DarjeelingGameJam
{
    /// <summary>
    /// Hides the system cursor
    /// </summary>
    public class CustomCursor : MonoBehaviour
    {
        private void Start()
        {
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
        }
    }
}
