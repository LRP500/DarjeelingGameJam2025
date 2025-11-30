using UnityEngine;
using Unity.Cinemachine;

namespace Modules.Toolbag.Variables.Binders
{
	[RequireComponent(typeof(CinemachineTargetGroup))]
	public class TargetGroupVariableBinder : VariableBinder<Transform>
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _weight = 1f;

		[SerializeField]
		private float _radius = 0f;

		private CinemachineTargetGroup _targetGroup;
		private int _targetIndex = -1;

		protected override void SetField(Transform value)
		{
			_targetGroup ??= GetComponent<CinemachineTargetGroup>();

			if (_targetIndex == -1)
			{
				_targetGroup.AddMember(value, _weight, _radius);
				_targetIndex = _targetGroup.FindMember(value);
			}
			else
			{
				_targetGroup.Targets[_targetIndex].Object = value;
			}
		}
	}
}
