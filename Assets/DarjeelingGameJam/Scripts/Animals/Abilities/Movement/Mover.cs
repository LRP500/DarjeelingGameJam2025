using UnityEngine;

namespace DarjeelingGameJam.Animals.Abilities
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Mover : AnimalAbility
    {
        protected Rigidbody2D Rigidbody { get; private set; }
        private Vector3 TargetPosition { get; set; }

        public override void Initialize()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        public void SetTargetPosition(Vector3 position)
        {
            TargetPosition = position;
        }

        private void FixedUpdate()
        {
            Move(TargetPosition);
        }

        protected abstract void Move(Vector3 position);
    }
}