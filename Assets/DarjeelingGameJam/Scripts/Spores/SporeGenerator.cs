using System;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarjeelingGameJam.Spores
{
    public class SporeGenerator : MonoBehaviour
    {
        [SerializeField]
        private Spore _spore;

        [MinValue(0)]
        [SerializeField]
        private float _interval;

        [Range(1, 10)]
        [SerializeField]
        [Tooltip("Nombre minimum de spores à générer")]
        private int _minSporeCount = 1;

        [Range(1, 10)]
        [SerializeField]
        [Tooltip("Nombre maximum de spores à générer")]
        private int _maxSporeCount = 10;

        [MinValue(0)]
        [SerializeField]
        [Tooltip("Rayon de la zone de spawn")]
        private float _spawnRadius = 1f;

        [SerializeField]
        private bool _simulateOnSpawn;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Probabilité pour chaque spore de germer (0.2 = 20% par spore)")]
        private float _germinationChancePerSpore = 0.2f;

        private IDisposable _disposable;
        private int _sporeSpawnedCount;
        private int _actualSporeCount; // Nombre réel de spores à générer (tiré aléatoirement)
        private bool[] _germinationStatus; // Tableau indiquant quelles spores peuvent germer

        private void OnEnable()
        {
            _sporeSpawnedCount = 0;

            // Tirer aléatoirement le nombre de spores à générer
            _actualSporeCount = Random.Range(_minSporeCount, _maxSporeCount + 1);

            // Pré-déterminer quelles spores seront germinantes (max 5)
            _germinationStatus = new bool[_actualSporeCount];
            int germinatingCount = 0;
            const int maxGerminatingSpores = 5;

            // Tirer au sort pour chaque spore individuellement
            for (int i = 0; i < _actualSporeCount; i++)
            {
                // Ne plus ajouter de spores germinantes si on a atteint le max de 5
                if (germinatingCount >= maxGerminatingSpores)
                    break;

                if (Random.value < _germinationChancePerSpore)
                {
                    _germinationStatus[i] = true;
                    germinatingCount++;
                }
            }

            // Garantir qu'au moins 1 spore sera germinante
            if (germinatingCount == 0)
            {
                int randomIndex = Random.Range(0, _actualSporeCount);
                _germinationStatus[randomIndex] = true;
            }

            _disposable = Observable
                .Interval(TimeSpan.FromSeconds(_interval))
                .Subscribe(_ => Generate());
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
            _disposable = null;
        }

        private void Generate()
        {
            var offset = Random.insideUnitCircle * _spawnRadius;

            var position = new Vector3(
                transform.position.x + offset.x,
                transform.position.y + offset.y);

            // Ne pas parenter les spores pour qu'elles gardent leur taille d'origine
            var spore = Instantiate(_spore, position, Quaternion.identity, null);

            // Utiliser le statut pré-déterminé pour cette spore
            bool canGerminate = _germinationStatus[_sporeSpawnedCount];
            spore.SetCanSpawnPlant(canGerminate);

            if (_simulateOnSpawn)
            {
                spore.Detach();
            }

            _sporeSpawnedCount++;

            if (_sporeSpawnedCount >= _actualSporeCount)
            {
                enabled = false;
            }
        }
    }
}