using UnityEngine;

namespace Modules.Toolbag.Extensions
{
    public static class CanvasGroupExtensions
    {
        public static void SetVisible(this CanvasGroup group, bool visible)
        {
            group.alpha = visible ? 1 : 0;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        public static bool IsVisible(this CanvasGroup group)
        {
            return group.alpha > 0;
        }
    }
}