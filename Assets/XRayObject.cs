using UnityEngine;

public class XRayObject : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool wrong;

    private void Start()
    {
        if (wrong)
        {
            spriteRenderer.color = Color.red;
        }

    }
}
