using UnityEngine;
using UnityEngine.Events;

public class GrowAnimationReceiver : MonoBehaviour
{
    public UnityEvent onEndGrow;

    public void EndGrow()
    {
        onEndGrow?.Invoke();
    }
}