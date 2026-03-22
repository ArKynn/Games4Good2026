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

    private GameObject _lastHoveredObject;

    private void LateUpdate() // Use LateUpdate to stop "vibrating" cursor
    {
        if (!active) return;

        UpdateVirtualCursor();
        HandleHoverDetection();

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

    private void HandleHoverDetection()
    {
        // 1. Setup pointer data at the virtual cursor's current UI position
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = virtualCursor.anchoredPosition;

        // 2. Raycast into the UI
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject currentHover = null;

        // 3. Find the first object that can handle pointer events (usually a Button or Image)
        foreach (var result in results)
        {
            // We look for the object or its parents (in case we hit a Text child)
            if (result.gameObject.GetComponentInParent<EventTrigger>() != null ||
                result.gameObject.GetComponentInParent<Selectable>() != null)
            {
                currentHover = result.gameObject;
                // If we hit a child, get the parent that actually holds the logic
                if (result.gameObject.GetComponent<EventTrigger>() == null)
                {
                    currentHover = result.gameObject.GetComponentInParent<EventTrigger>()?.gameObject ??
                                   result.gameObject.GetComponentInParent<Selectable>()?.gameObject;
                }
                break;
            }
        }

        // 4. Handle State Changes
        if (currentHover != _lastHoveredObject)
        {
            // --- POINTER EXIT ---
            if (_lastHoveredObject != null)
            {
                ExecuteEvents.Execute(_lastHoveredObject, pointerData, ExecuteEvents.pointerExitHandler);
            }

            // --- POINTER ENTER ---
            if (currentHover != null)
            {
                ExecuteEvents.Execute(currentHover, pointerData, ExecuteEvents.pointerEnterHandler);
            }

            _lastHoveredObject = currentHover;
        }
    }

    public void OnMouseClick()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, screenLayer))
        {
            if (hit.collider.transform != this.transform)
            {
                return;
            }
        }
        else
            return;

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
                Debug.Log($"Clicked on button: {button.gameObject.name}");
                button.onClick.Invoke();
                break;
            }
        }
    }
}