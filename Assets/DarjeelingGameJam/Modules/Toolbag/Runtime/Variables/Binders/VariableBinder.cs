using Modules.Toolbag.Extensions;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modules.Toolbag.Variables.Binders
{
	public abstract class VariableBinder<T> : MonoBehaviour
	{
		[SerializeField]
		[Required]
		private ReactiveVariable<T> _source;

		private void OnEnable()
		{
			_source.Property
				.Subscribe(OnValueTargetChange)
				.AddToOnDisable(this);
		}

		private void OnValueTargetChange(T value)
		{
			SetField(value);
		}

		protected abstract void SetField(T value);
	}
}
