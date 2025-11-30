using UnityEngine;

namespace Modules.Toolbag.Misc
{
	public static class Maths
	{
		public static Vector3 Projection(Vector3 a, Vector3 b, Vector3 toProject)
		{
			Vector3 ab = b - a;
			Vector3 atoProject = toProject - a;
			float t = Vector3.Dot(atoProject, ab) / Vector3.Dot(ab, ab);
			t = Mathf.Clamp01(t);

			return a + t * ab;
		}
	}
}