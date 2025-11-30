using UnityEngine.SceneManagement;

namespace Modules.Toolbag.Utils
{
	public static class SceneUtils
	{
		public static bool IsLoaded(this int sceneIndex)
		{
			if (sceneIndex == -1)
			{
				return false;
			}
			
			var scene = SceneManager.GetSceneByBuildIndex(sceneIndex);

			if (scene != default)
			{
				return scene.IsValid() && scene.isLoaded;
			}
			
			return false;
		}
		
		public static void SetActiveScene(string sceneName)
		{
			var scene = SceneManager.GetSceneByName(sceneName);

			if (scene != default)
			{
				SceneManager.SetActiveScene(scene);
			}
		}
	}
}