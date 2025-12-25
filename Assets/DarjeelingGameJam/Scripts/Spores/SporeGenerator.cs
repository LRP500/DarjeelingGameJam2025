using System;
using DarjeelingGameJam.Plants;
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
        [Tooltip("Nombre minimum de spores à générer (valeur de base, début du jeu)")]
        private int _minSporeCount = 1;

        [Range(1, 10)]
        [SerializeField]
        [Tooltip("Nombre maximum de spores à générer (valeur de base, début du jeu)")]
        private int _maxSporeCount = 10;

        [MinValue(0)]
        [SerializeField]
        [Tooltip("Rayon de la zone de spawn")]
        private float _spawnRadius = 1f;

        [SerializeField]
        private bool _simulateOnSpawn;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Probabilité pour chaque spore de germer (valeur de base, début du jeu)")]
        private float _germinationChancePerSpore = 0.2f;

        [Header("Scaling Dynamique (selon nombre de plantes)")]
        [Tooltip("Nombre de plantes où commencer à réduire la génération")]
        [SerializeField]
        private int _plantCountThresholdStart = 300;

        [Tooltip("Nombre de plantes où atteindre le minimum de génération")]
        [SerializeField]
        private int _plantCountThresholdMax = 1000;

        [Range(0f, 1f)]
        [Tooltip("Multiplicateur minimum pour le nombre de spores (0.5 = 50% des spores au max)")]
        [SerializeField]
        private float _minSporeMultiplier = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("Multiplicateur minimum pour la germination (0.3 = 30% au max)")]
        [SerializeField]
        private float _minGerminationMultiplier = 0.3f;

        private IDisposable _disposable;
        private int _sporeSpawnedCount;
        private int _actualSporeCount; // Nombre réel de spores à générer (tiré aléatoirement)
        private bool[] _germinationStatus; // Tableau indiquant quelles spores peuvent germer

        private void OnEnable()
        {
            _sporeSpawnedCount = 0;

            // Calculer les multiplicateurs selon le nombre de plantes actives
            int plantCount = PlantCounter.Count;
            float sporeMultiplier = CalculateSporeMultiplier(plantCount);
            float germinationMultiplier = CalculateGerminationMultiplier(plantCount);

            Debug.Log($"[SporeGenerator] PlantCount={plantCount}, SporeMulti={sporeMultiplier:F2}, GermMulti={germinationMultiplier:F2}, Base={_minSporeCount}-{_maxSporeCount}");

            // Appliquer le scaling au nombre de spores (garde tes valeurs de base au début)
            int scaledMinCount = Mathf.Max(1, Mathf.RoundToInt(_minSporeCount * sporeMultiplier));
            int scaledMaxCount = Mathf.Max(1, Mathf.RoundToInt(_maxSporeCount * sporeMultiplier));

            Debug.Log($"[SporeGenerator] Scaled counts: {scaledMinCount}-{scaledMaxCount}");

            // Tirer aléatoirement le nombre de spores à générer (ajusté)
            _actualSporeCount = Random.Range(scaledMinCount, scaledMaxCount + 1);

            // Appliquer le scaling à la germination (garde ta valeur de base au début)
            float scaledGerminationChance = _germinationChancePerSpore * germinationMultiplier;

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

                if (Random.value < scaledGerminationChance)
                {
                    _germinationStatus[i] = true;
                    germinatingCount++;
                }
            }

            // Garantir qu'au moins 1 spore sera germinante EN DESSOUS de 1000 plantes
            // AU-DELÀ de 1000 plantes, il peut y avoir 0 spore germinante (ralentit la croissance)
            if (germinatingCount == 0 && plantCount < _plantCountThresholdMax)
            {
                int randomIndex = Random.Range(0, _actualSporeCount);
                _germinationStatus[randomIndex] = true;
            }

            _disposable = Observable
                .Interval(TimeSpan.FromSeconds(_interval))
                .Subscribe(_ => Generate());
        }

        /// <summary>
        /// Calcule le multiplicateur de nombre de spores selon le nombre de plantes.
        /// Retourne 1.0 au début (utilise tes valeurs), descend progressivement.
        /// </summary>
        private float CalculateSporeMultiplier(int plantCount)
        {
            if (plantCount < _plantCountThresholdStart)
                return 1f; // 100% de tes valeurs de base

            if (plantCount >= _plantCountThresholdMax)
                return _minSporeMultiplier; // Minimum (ex: 20%)

            // Interpolation linéaire entre start et max
            float t = (float)(plantCount - _plantCountThresholdStart) / (_plantCountThresholdMax - _plantCountThresholdStart);
            return Mathf.Lerp(1f, _minSporeMultiplier, t);
        }

        /// <summary>
        /// Calcule le multiplicateur de germination selon le nombre de plantes.
        /// La germination reste à 100% jusqu'à plantCountThresholdMax (1000), puis descend après.
        /// </summary>
        private float CalculateGerminationMultiplier(int plantCount)
        {
            // Pas de réduction de germination en dessous de 1000 plantes
            if (plantCount < _plantCountThresholdMax)
                return 1f; // 100% de ta valeur de base

            // Au-delà de 1000 plantes, commence à réduire
            // On peut continuer à descendre jusqu'à un certain seuil
            int germinationMaxThreshold = _plantCountThresholdMax + 500; // 1500 plantes

            if (plantCount >= germinationMaxThreshold)
                return _minGerminationMultiplier; // Minimum (ex: 30%)

            // Interpolation linéaire entre 1000 et 1500
            float t = (float)(plantCount - _plantCountThresholdMax) / (germinationMaxThreshold - _plantCountThresholdMax);
            return Mathf.Lerp(1f, _minGerminationMultiplier, t);
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