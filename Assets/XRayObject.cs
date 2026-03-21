using UnityEngine;

public class XRayObject : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool wrong;

    private void Start()
    {
        if (wrong)
        {
            spriteRenderer.color = new Color(1f, 0.75f, 0.75f); // Light Red for wrong objects
        }

    }
}
