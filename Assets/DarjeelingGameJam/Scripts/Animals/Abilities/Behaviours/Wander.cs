using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        private CancellationTokenSource _tokenSource;
        private BoxCollider2D _confiner;
        
        public override void Initialize()
        {
            _mover = Owner.GetAbility<Mover>();
        }

        private void OnEnable()
        {
            _tokenSource = new CancellationTokenSource();

            RunDecisionLoop(_tokenSource.Token).Forget();
        }
        
        private void OnDisable()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
            
            _mover.SetTargetPosition(transform.position);
        }
        
        private async UniTask RunDecisionLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var targetPosition = PickRandomPosition();
                _mover.SetTargetPosition(targetPosition);
                await Task.Delay(TimeSpan.FromSeconds(_decisionTime), cancellationToken: token);
            }
        }

        private Vector3 PickRandomPosition()
        {
            _confiner = _confinerReference.Value.GetComponent<BoxCollider2D>();
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