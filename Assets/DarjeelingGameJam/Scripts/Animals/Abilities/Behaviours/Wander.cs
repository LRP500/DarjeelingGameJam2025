using Modules.Toolbag.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarjeelingGameJam.Animals.Abilities
{
    public class Wander : MovementBehaviour
    {
        [SerializeField]
        private TransformReactiveVariable _confinerReference;
        
        [MinValue(0)]
        [SerializeField]
        private float _range = 10f;

        [MinValue(0.1)]
        [SerializeField]
        private float _decisionTime;

        private Mover _mover;
        private BoxCollider2D _confiner;
        private float _timer;
        
        public override void Initialize()
        {
            _mover = Owner.GetAbility<Mover>();
            _confiner = transform.parent.GetComponentInChildren<BoxCollider2D>();
        }

        private void OnDisable()
        {
            _mover.SetTargetPosition(transform.position);
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= _decisionTime)
            {
                UpdateTargetPosition();
                _timer = 0;
            }
        }

        private void UpdateTargetPosition()
        {
            var targetPosition = PickRandomPosition();
            _mover.SetTargetPosition(targetPosition);
        }
        
        private Vector3 PickRandomPosition()
        {
            var random = Random.insideUnitCircle * _range;
            var position = transform.position + new Vector3(random.x, random.y, 0);
            position.x = Mathf.Clamp(position.x, _confiner.bounds.min.x, _confiner.bounds.max.x);
            position.y = Mathf.Clamp(position.y, _confiner.bounds.min.y, _confiner.bounds.max.y);
            return position;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gold;
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}