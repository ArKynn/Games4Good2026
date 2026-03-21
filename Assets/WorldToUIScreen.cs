using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldToUIScreen : MonoBehaviour
{
    [Header("Activation")]
    public bool active = false;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask screenLayer;

    [Header("Smoothing")]
    [Range(1f, 500f)]
    [SerializeField] private float smoothSpeed = 15f; // Higher is faster/snappier, lower is smoother/slower

    [Header("References")]
    [SerializeField] private Camera uiCamera;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform virtualCursor;

    private Vector2 _targetPos;

    private void LateUpdate() // Use LateUpdate to stop "vibrating" cursor
    {
        if (!active) return;

        UpdateVirtualCursor();

        if (Input.GetMouseButtonDown(0))
        {
            OnMouseClick();
        }
    }

    private void UpdateVirtualCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, screenLayer))
        {
            if (hit.collider.transform == this.transform)
            {
                // Remove Lerp entirely for zero delay
                Vector2 localPoint = hit.textureCoord;

                _targetPos = new Vector2(
                    localPoint.x * canvasRect.sizeDelta.x,
                    localPoint.y * canvasRect.sizeDelta.y
                );

                if (virtualCursor != null)
                {
                    virtualCursor.anchoredPosition = _targetPos;

                    if (!virtualCursor.gameObject.activeSelf)
                        virtualCursor.gameObject.SetActive(true);
                }
            }
        }
    }

    public void OnMouseClick()
    {
        // For clicking, we use the EXACT current position of the virtual cursor 
        // This ensures that if the user clicks while the cursor is still smoothing/moving, 
        // the click happens where the visual cursor actually is.
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = virtualCursor.anchoredPosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            Button button = result.gameObject.GetComponentInParent<Button>();
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
                break;
            }
        }
    }
}