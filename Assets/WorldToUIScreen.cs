using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldToUIScreen : MonoBehaviour
{
    [Header("Activation")]
    public bool active = false;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask screenLayer; // Set this to "ScreenUI" in the Inspector

    [Header("References")]
    [SerializeField] private Camera uiCamera;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform virtualCursor;

    private void Update()
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
        // Added the LayerMask to the Raycast call
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, screenLayer))
        {
            if (hit.collider.transform == this.transform)
            {
                Vector2 localPoint = hit.textureCoord;

                Vector2 screenPos = new Vector2(
                    localPoint.x * canvasRect.sizeDelta.x,
                    localPoint.y * canvasRect.sizeDelta.y
                );

                if (virtualCursor != null)
                {
                    virtualCursor.anchoredPosition = screenPos;

                    // Optional: Make cursor visible only when hitting the screen
                    if (!virtualCursor.gameObject.activeSelf)
                        virtualCursor.gameObject.SetActive(true);
                }
            }
        }
        else if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
        {
            // Hide cursor if we look away from the screen
            virtualCursor.gameObject.SetActive(false);
        }
    }

    public void OnMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Added the LayerMask here as well
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, screenLayer))
        {
            if (hit.collider.transform != this.transform) return;

            Vector2 localPoint = hit.textureCoord;
            Vector2 screenPos = new Vector2(
                localPoint.x * canvasRect.sizeDelta.x,
                localPoint.y * canvasRect.sizeDelta.y
            );

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = screenPos;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                Button button = result.gameObject.GetComponentInParent<Button>();

                if (button != null && button.interactable)
                {
                    Debug.Log("Directly Invoking: " + button.gameObject.name);
                    button.onClick.Invoke();
                    break;
                }
            }
        }
    }
}