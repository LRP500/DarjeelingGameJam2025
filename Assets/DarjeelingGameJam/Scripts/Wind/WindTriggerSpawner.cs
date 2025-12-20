using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// Manages pooling and spawning of wind physics triggers.
    /// Separated from visual rendering for cleaner architecture.
    /// </summary>
    public class WindTriggerSpawner : MonoBehaviour
    {
        [Header("Trigger Pooling")]
        [Required]
        [Tooltip("Prefab containing CircleCollider2D and WindForceTrigger")]
        [SerializeField]
        private GameObject _triggerPrefab;

        [Tooltip("Number of triggers to pre-instantiate in the pool")]
        [MinValue(5)]
        [SerializeField]
        private int _triggerPoolSize = 20;

        [Header("Collider Settings")]
        [Tooltip("Base radius for trigger colliders (scaled by velocity)")]
        [MinValue(0.1f)]
        [SerializeField]
        private float _baseColliderRadius = 0.5f;

        [Tooltip("Maximum radius multiplier at max velocity")]
        [MinValue(1f)]
        [SerializeField]
        private float _maxRadiusMultiplier = 1.6f;

        private Queue<GameObject> _triggerPool;
        private List<GameObject> _activeTriggers;

        private void Awake()
        {
            InitializeTriggerPool();
        }

        private void InitializeTriggerPool()
        {
            _triggerPool = new Queue<GameObject>(_triggerPoolSize);
            _activeTriggers = new List<GameObject>(_triggerPoolSize);

            // Pre-instantiate triggers
            for (int i = 0; i < _triggerPoolSize; i++)
            {
                GameObject trigger = Instantiate(_triggerPrefab, transform);
                trigger.SetActive(false);
                _triggerPool.Enqueue(trigger);
            }
        }

        /// <summary>
        /// Spawns physics trigger at specified position
        /// </summary>
        public void SpawnWindTrigger(
            Vector3 position,
            Vector2 direction,
            float forceMultiplier,
            float lifetime,
            float velocityScale)
        {
            // Get trigger from pool or create new one if pool is empty
            GameObject trigger = GetTriggerFromPool();
            if (trigger == null)
                return;

            // Position trigger
            trigger.transform.position = position;
            trigger.SetActive(true);

            // Configure collider size based on velocity
            CircleCollider2D collider = trigger.GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                float radiusMultiplier = Mathf.Lerp(1f, _maxRadiusMultiplier, velocityScale);
                collider.radius = _baseColliderRadius * radiusMultiplier;
                collider.isTrigger = true;
            }

            // Initialize wind force trigger
            WindForceTrigger forceTrigger = trigger.GetComponent<WindForceTrigger>();
            if (forceTrigger != null)
            {
                forceTrigger.Initialize(direction, forceMultiplier, lifetime);
            }

            // Track active trigger
            _activeTriggers.Add(trigger);

            // Return to pool after lifetime
            StartCoroutine(ReturnTriggerToPoolAfterDelay(trigger, lifetime));
        }

        private GameObject GetTriggerFromPool()
        {
            if (_triggerPool.Count > 0)
            {
                return _triggerPool.Dequeue();
            }

            // Pool exhausted - create temporary trigger (will be destroyed, not pooled)
            Debug.LogWarning($"[WindTriggerSpawner] Trigger pool exhausted! Consider increasing pool size. Current: {_triggerPoolSize}");
            GameObject tempTrigger = Instantiate(_triggerPrefab, transform);
            return tempTrigger;
        }

        private IEnumerator ReturnTriggerToPoolAfterDelay(GameObject trigger, float delay)
        {
            yield return new WaitForSeconds(delay);

            // Deactivate and return to pool
            if (trigger != null)
            {
                trigger.SetActive(false);
                _activeTriggers.Remove(trigger);

                // Only return to pool if it's one of our pooled objects
                if (trigger.transform.parent == transform)
                {
                    _triggerPool.Enqueue(trigger);
                }
                else
                {
                    // Destroy temporary triggers
                    Destroy(trigger);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            // Draw active triggers
            if (_activeTriggers != null)
            {
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                foreach (var trigger in _activeTriggers)
                {
                    if (trigger != null && trigger.activeInHierarchy)
                    {
                        CircleCollider2D collider = trigger.GetComponent<CircleCollider2D>();
                        if (collider != null)
                        {
                            Gizmos.DrawWireSphere(trigger.transform.position, collider.radius);
                        }
                    }
                }
            }

#if UNITY_EDITOR
            // Draw pool status in editor
            if (_triggerPool != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.cyan;
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 2,
                    $"Triggers: {_triggerPool.Count}/{_triggerPoolSize} | Active: {_activeTriggers?.Count ?? 0}",
                    style
                );
            }
#endif
        }
    }
}
