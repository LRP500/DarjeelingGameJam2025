using Modules.Toolbag.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DarjeelingGameJam.Animals.Abilities
{
    public class SimpleMover : Mover
    {
        [MinValue(0)]
        [SerializeField]
        private float _speed;

        [MinValue(0)]
        [SerializeField]
        private float _mouseSpeedMultiplier = 2f;
        
        [MinValue(0.01f)]
        [SerializeField]
        private float _stoppingDistance = 0.1f;
        
        protected override void Move(Vector3 position)
        {
            if (transform.position.AlmostEqual(position, _stoppingDistance))
            {
                Rigidbody.linearVelocity = Vector3.zero;
                return;
            }

            var direction = position - transform.position;
            var movement = direction.normalized * (GetSpeed() * Time.fixedDeltaTime);
            Rigidbody.MovePosition(transform.position + movement);
            Rigidbody.linearVelocity = movement;
        }

        private float GetSpeed()
        {
            var moveToMouse = Owner.GetAbility<MoveToMouse>();
            return _speed * (moveToMouse.enabled ? _mouseSpeedMultiplier : 1);
        }
    }
}