using UnityEngine;
using DG.Tweening;

public class SuitcaseMinigame : MonoBehaviour
{
    [Header("Activation Settings")]
    public bool active;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private GameObject instructionsUI;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    [Header("Physics Drag Settings")]
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private float dragSpeed = 12f;

    private Rigidbody _grabbedRb;
    private float _dragDepth;
    private Vector3 _grabOffset;

    public void ActivateGame(bool toggle)
    {
        active = toggle;

        if (toggle)
        {
            // Prepare for transition
            FirstPersonViewport.Instance.minigameActive = true;
            FirstPersonViewport.Instance.SetMovementActive(false);
            originalCameraPosition = Camera.main.transform.position;
            originalCameraRotation = Camera.main.transform.rotation;

            // Move Camera to Minigame Position
            Camera.main.transform.DOMove(cameraPosition.position, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (instructionsUI != null)
                {
                    instructionsUI.SetActive(true);
                    instructionsUI.transform.localScale = Vector3.zero;
                    instructionsUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                }
            });
            Camera.main.transform.DORotate(cameraPosition.eulerAngles, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            FirstPersonViewport.Instance.minigameActive = true;
            // Release any grabbed object
            Release();

            // Hide UI
            if (instructionsUI != null)
            {
                instructionsUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    instructionsUI.SetActive(false);
                });
            }

            // Return Camera to Player
            Camera.main.transform.DOMove(originalCameraPosition, 0.5f).SetEase(Ease.InBack);
            Camera.main.transform.DORotate(originalCameraRotation.eulerAngles, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                FirstPersonViewport.Instance.SetMovementActive(true);
            });
        }
    }

    void Update()
    {
        // Exit minigame with Escape
        if (Input.GetKeyDown(KeyCode.Escape) && active)
        {
            ActivateGame(false);
        }

        if (!active) return;

        // Handle Dragging Input
        if (Input.GetMouseButtonDown(0))
        {
            TryGrab();
        }

        if (Input.GetMouseButtonUp(0))
        {
            Release();
        }
    }

    void FixedUpdate()
    {
        if (active && _grabbedRb != null)
        {
            HandlePhysicsDrag();
        }
    }

    [Header("Physics Tuning")]
    [SerializeField] private float dragForce = 20f;      // How "strong" the stickiness is
    [SerializeField] private float maxVelocity = 10f;    // The speed limit (prevents flying away)
    [SerializeField] private float damping = 5f;        // How fast it stops wobbling

    private void TryGrab()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableLayer))
        {
            if (hit.collider.TryGetComponent(out TAG_CaseLead lead))
            {
                _grabbedRb = hit.collider.GetComponent<Rigidbody>();
                if (_grabbedRb != null)
                {
                    _dragDepth = Vector3.Distance(Camera.main.transform.position, hit.point);
                    _grabOffset = _grabbedRb.transform.position - hit.point;

                    _grabbedRb.useGravity = false;

                    // IMPORTANT: High damping makes it feel "heavy" and prevents sliding
                    _grabbedRb.linearDamping = damping;
                    _grabbedRb.angularDamping = damping;
                }
            }
        }
    }

    private void HandlePhysicsDrag()
    {
        // 1. Calculate Target Position
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = _dragDepth;
        Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(mousePos) + _grabOffset;

        // 2. Calculate the "Spring" Force
        // Instead of setting velocity, we add to it based on distance
        Vector3 diff = targetWorldPos - _grabbedRb.transform.position;

        // Apply the force
        _grabbedRb.linearVelocity = diff * dragForce;

        // 3. The "Speed Limit" (The Fix for flying away)
        // If the velocity exceeds our max, we trim it down
        if (_grabbedRb.linearVelocity.magnitude > maxVelocity)
        {
            _grabbedRb.linearVelocity = _grabbedRb.linearVelocity.normalized * maxVelocity;
        }
    }

    private void Release()
    {
        if (_grabbedRb != null)
        {
            _grabbedRb.useGravity = true;
            _grabbedRb.linearDamping = 0.05f;

            // Force the physics engine to stop moving it if it's slow
            if (_grabbedRb.linearVelocity.magnitude < 0.5f)
            {
                _grabbedRb.linearVelocity = Vector3.zero;
                _grabbedRb.angularVelocity = Vector3.zero;
                _grabbedRb.Sleep();
            }

            _grabbedRb = null;
        }
    }
}