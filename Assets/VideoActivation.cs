using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    public void Activate()
    {
        targetObject.SetActive(true);
    }
}