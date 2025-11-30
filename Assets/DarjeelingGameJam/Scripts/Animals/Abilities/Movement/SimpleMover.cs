using Cysharp.Threading.Tasks;
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

        [MinValue(0.01f)]
        [SerializeField]
        private float _stoppingDistance = 0.1f;
        
        protected override void Move(Vector3 position)
        {
            if (transform.position.AlmostEqualXY(position, _stoppingDistance))
            {
                return;
            }

            var direction = position - transform.position;
            var movement = direction.normalized * (_speed * Time.fixedDeltaTime);
            Rigidbody.MovePosition(transform.position + movement);
        }

        public override UniTask Enable()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask Disable()
        {
            return UniTask.CompletedTask;
        }
    }
}