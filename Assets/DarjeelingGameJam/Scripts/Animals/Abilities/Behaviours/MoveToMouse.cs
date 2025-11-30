using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Animals.Abilities
{
    public class MoveToMouse : MovementBehaviour
    {
        [MinValue(0.1f)]
        [SerializeField]
        private float _refreshRate = 0.5f;

        private Mover _mover;
        private Camera _camera;
        private CancellationTokenSource _tokenSource;
        
        public override void Initialize()
        {
            _camera = Camera.main;
            _mover = Owner.GetAbility<Mover>();
        }

        private void OnEnable()
        {
            _tokenSource = new CancellationTokenSource();
            RunProcess().Forget();
        }

        private void OnDisable()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        private async UniTaskVoid RunProcess()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                UpdateTargetDestination();
                await Task.Delay(TimeSpan.FromSeconds(_refreshRate), _tokenSource.Token);
            }
        }

        private void UpdateTargetDestination()
        {
            var mousePosition = Mouse.current.position.ReadValue();
            var targetPosition = _camera.ScreenToWorldPoint(mousePosition);
            _mover.SetTargetPosition(targetPosition);
        }
    }
}