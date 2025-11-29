using System;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarjeelingGameJam.Spores
{
    [RequireComponent(typeof(SphereCollider))]
    public class SporeGenerator : MonoBehaviour
    {
        [SerializeField]
        private Spore _spore;

        [MinValue(0)]
        [SerializeField]
        private float _interval;

        private SphereCollider _collider;
        private IDisposable _disposable;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
        }

        private void OnEnable()
        {
            _disposable = Observable
                .Interval(TimeSpan.FromSeconds(_interval))
                .Subscribe(_ => Generate());
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Generate()
        {
            var offset = Random.insideUnitCircle * _collider.radius;

            var position = new Vector3(
                transform.position.x + offset.x,
                transform.position.y + offset.y);

            Instantiate(_spore, position, Quaternion.identity, transform);
        }
    }
}