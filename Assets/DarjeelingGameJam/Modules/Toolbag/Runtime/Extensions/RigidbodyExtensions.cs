using UnityEngine;

namespace Modules.Toolbag.Extensions
{
	[RequireComponent(typeof(Rigidbody))]
	public class RigidbodyExtensions : MonoBehaviour
	{
		private Rigidbody _rigidbody;

		private void Start()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}

		public void FreezePositionX(bool freeze)
		{
			var freezeSituation = freeze ? RigidbodyConstraints.FreezePositionX : ~RigidbodyConstraints.FreezePositionX;
			_rigidbody.constraints &= freezeSituation;
		}

		public void FreezePositionY(bool freeze)
		{
			var freezeSituation = freeze ? RigidbodyConstraints.FreezePositionY : ~RigidbodyConstraints.FreezePositionY;
			_rigidbody.constraints &= freezeSituation;
		}

		public void FreezePositionZ(bool freeze)
		{
			var freezeSituation = freeze ? RigidbodyConstraints.FreezePositionZ : ~RigidbodyConstraints.FreezePositionZ;
			_rigidbody.constraints &= freezeSituation;
		}

		public void FreezeRotationX(bool freeze)
		{
			var freezeSituation = freeze ? RigidbodyConstraints.FreezeRotationX : ~RigidbodyConstraints.FreezeRotationX;
			_rigidbody.constraints &= freezeSituation;
		}

		public void FreezeRotationY(bool freeze)
		{
			var freezeSituation = freeze ? RigidbodyConstraints.FreezeRotationY : ~RigidbodyConstraints.FreezeRotationY;
			_rigidbody.constraints &= freezeSituation;
		}

		public void FreezeRotationZ(bool freeze)
		{
			var freezeSituation = freeze ? RigidbodyConstraints.FreezeRotationZ : ~RigidbodyConstraints.FreezeRotationZ;
			_rigidbody.constraints &= freezeSituation;
		}
	}
}