using UnityEngine;
using Unity.Cinemachine;

namespace Modules.Toolbag.Variables.Binders
{
	[RequireComponent(typeof(CinemachineCamera))]
	public class FollowTransformVariableBinder : VariableBinder<Transform>
	{
		private CinemachineCamera _camera;

		protected override void SetField(Transform value)
		{
			_camera ??= GetComponent<CinemachineCamera>();
			_camera.Follow = value;
		}
	}
}
