using UnityEngine;

namespace DarjeelingGameJam.Parallax
{
    public class SetChildrenLayerOrder : MonoBehaviour
    {
        [SerializeField]
        private int _orderInLayer;

        private void Awake()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = _orderInLayer;
            }
        }
    }
}