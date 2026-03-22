using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] public UnityEvent onInteract;

    public Outline outline;

    private IEnumerator Start()
    {

        yield return new WaitForSeconds(0.01f); // Wait a frame to ensure all components are initialized

        if(outline == null)
            outline = GetComponent<Outline>();  

        if(outline != null)
            outline.enabled = false;
    }

    public void Interact()
    {
        onInteract.Invoke();

    }
}
