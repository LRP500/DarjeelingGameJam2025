using Sirenix.Utilities;
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
            renderers.ForEach(x => x.sortingOrder = _orderInLayer);
        }
    }
}