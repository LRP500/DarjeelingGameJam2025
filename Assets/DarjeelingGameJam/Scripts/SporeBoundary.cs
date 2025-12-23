using DarjeelingGameJam.Spores;
using UnityEngine;

namespace DarjeelingGameJam
{
    /// <summary>
    /// Keeps spores within a boundary by applying a force pushing them back inside
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SporeBoundary : MonoBehaviour
    {
        [Tooltip("Force applied to push spores back inside")]
        [SerializeField]
        private float _pushForce = 5f;

        [Tooltip("Use center of bounds as target (true) or closest point inside (false)")]
        [SerializeField]
        private bool _pushTowardsCenter = false;

        private Collider2D _boundary;

        private void Awake()
        {
            _boundary = GetComponent<Collider2D>();

            // Ensure it's a trigger
            if (!_boundary.isTrigger)
            {
                Debug.LogWarning("SporeBoundary: Collider should be a trigger. Setting it to trigger.");
                _boundary.isTrigger = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Check if it's a spore
            Spore spore = other.GetComponent<Spore>();
            if (spore != null && spore.IsDetached)
            {
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 sporePos = other.transform.position;
                    Vector2 targetPos;

                    if (_pushTowardsCenter)
                    {
                        // Push towards center of bounds
                        targetPos = _boundary.bounds.center;
                    }
                    else
                    {
                        // Push towards closest point inside the boundary
                        targetPos = _boundary.ClosestPoint(sporePos);
                    }

                    // Calculate direction and apply force
                    Vector2 direction = (targetPos - sporePos).normalized;
                    rb.AddForce(direction * _pushForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}
