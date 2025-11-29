using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reçoit l'AnimationEvent "EndGrow" sur l'animation "Fleur".
/// Mets ce script sur le même GameObject que l'Animator.
/// </summary>
public class GrowAnimationReceiver : MonoBehaviour
{
    [Tooltip("Appelé quand l'AnimationEvent 'EndGrow' est déclenché.")]
    public UnityEvent onEndGrow;

    // DOIT matcher EXACTEMENT le nom de l'AnimationEvent : EndGrow
    public void EndGrow()
    {
        Debug.Log($"[GrowAnimationReceiver] EndGrow reçu sur {gameObject.name}");
        onEndGrow?.Invoke();
    }
}
