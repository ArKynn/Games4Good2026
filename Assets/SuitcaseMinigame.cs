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

    [Header("Scroll & Rotate Settings")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float minScrollDistance = 0.5f;
    [SerializeField] private float maxScrollDistance = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    private Rigidbody _grabbedRb;
    private float _dragDepth;
    private Vector3 _grabOffset;
    private bool _isCaseLead;
    private bool _isRotating; // Track if RMB is held

    public void ActivateGame(bool toggle)
    {
        active = toggle;
        if (toggle)
        {
            FirstPersonViewport.Instance.minigameActive = true;
            FirstPersonViewport.Instance.SetMovementActive(false);
            originalCameraPosition = Camera.main.transform.position;
            originalCameraRotation = Camera.main.transform.rotation;

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
            FirstPersonViewport.Instance.minigameActive = false;
            Release();
            if (instructionsUI != null)
            {
                instructionsUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    instructionsUI.SetActive(false);
                });
            }
            Camera.main.transform.DOMove(originalCameraPosition, 0.5f).SetEase(Ease.InBack);
            Camera.main.transform.DORotate(originalCameraRotation.eulerAngles, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                FirstPersonViewport.Instance.SetMovementActive(true);
            });
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && active)
        {
            ActivateGame(false);
        }

        if (!active) return;

        if (Input.GetMouseButtonDown(0)) TryGrab();
        if (Input.GetMouseButtonUp(0)) Release();

        if (_grabbedRb != null && !_isCaseLead)
        {
            // --- SCROLL LOGIC ---
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _dragDepth = Mathf.Clamp(_dragDepth + (scroll * scrollSpeed), minScrollDistance, maxScrollDistance);
            }

            // --- ROTATION TOGGLE ---
            // Only allow rotation if it's not the Case Lead
            _isRotating = Input.GetMouseButton(1);
        }
    }

    void FixedUpdate()
    {
        if (active && _grabbedRb != null)
        {
            if (_isRotating)
            {
                HandleRotation();
            }
            else
            {
                HandlePhysicsDrag();
            }
        }
    }

    [Header("Physics Tuning")]
    [SerializeField] private float dragForce = 20f;
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float damping = 5f;

    private void TryGrab()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableLayer))
        {
            _grabbedRb = hit.collider.GetComponent<Rigidbody>();
            if (_grabbedRb != null)
            {
                _isCaseLead = _grabbedRb.GetComponent<TAG_CaseLead>() != null;
                _dragDepth = Vector3.Distance(Camera.main.transform.position, hit.point);
                _grabOffset = _grabbedRb.transform.position - hit.point;

                _grabbedRb.useGravity = false;
                _grabbedRb.linearDamping = damping;
                _grabbedRb.angularDamping = damping;
            }
        }
    }

    private void HandlePhysicsDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = _dragDepth;
        Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(mousePos) + _grabOffset;

        Vector3 diff = targetWorldPos - _grabbedRb.transform.position;
        _grabbedRb.linearVelocity = diff * dragForce;

        if (_grabbedRb.linearVelocity.magnitude > maxVelocity)
        {
            _grabbedRb.linearVelocity = _grabbedRb.linearVelocity.normalized * maxVelocity;
        }
    }

    private void HandleRotation()
    {
        // Stop movement while rotating so it doesn't drift away
        _grabbedRb.linearVelocity = Vector3.zero;

        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Rotate relative to the Camera's up and right axes for intuitive feel
        _grabbedRb.AddTorque(Camera.main.transform.up * -mouseX, ForceMode.VelocityChange);
        _grabbedRb.AddTorque(Camera.main.transform.right * mouseY, ForceMode.VelocityChange);
    }

    private void Release()
    {
        if (_grabbedRb != null)
        {
            _grabbedRb.useGravity = true;
            _grabbedRb.linearDamping = 0.05f;
            _grabbedRb.angularDamping = 0.05f;

            if (_grabbedRb.linearVelocity.magnitude < 0.5f && _isCaseLead)
            {
                _grabbedRb.linearVelocity = Vector3.zero;
                _grabbedRb.angularVelocity = Vector3.zero;

            }

            _grabbedRb = null;
            _isCaseLead = false;
            _isRotating = false;
        }
    }
}