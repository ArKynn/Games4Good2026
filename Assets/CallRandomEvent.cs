using UnityEngine;
using UnityEngine.Events;

public class CallRandomEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent onCallEvent;
    public void CallEvent()
    {
        onCallEvent.Invoke();
    }
}
