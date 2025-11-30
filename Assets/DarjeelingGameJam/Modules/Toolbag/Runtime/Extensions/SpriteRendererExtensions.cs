using UnityEngine;

namespace Modules.Toolbag.Extensions
{
    public static class SpriteRendererExtensions
    {
        /// <summary>
        /// Set alpha value of sprite renderer's color.
        /// </summary>
        /// <param name="self">self</param>
        /// <param name="alpha">alpha</param>
        public static void SetAlpha(this SpriteRenderer self, float alpha)
        {
            var next = self.color;
            next.a = Mathf.Clamp01(alpha);
            self.color = next;
        }
    }
}