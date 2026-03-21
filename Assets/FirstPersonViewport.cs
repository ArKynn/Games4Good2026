using UnityEngine;

public class FirstPersonViewport : MonoBehaviour
{
    // --- SINGLETON PATTERN ---
    public static FirstPersonViewport Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    // -------------------------

    [Header("Settings")]
    [SerializeField] private bool canMove = true; // Toggle this in Inspector or code
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] Vector2 yawLimit = new Vector2(-60f, 60f);
    [SerializeField] Vector2 pitchLimit = new Vector2(-30f, 30f);

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private float _currentYaw;
    private float _currentPitch;

    private GameObject lastSeenObjectGO;
    private Outline lastSeenObjectOutline;

    public bool minigameActive = false;

    void Start()
    {
        Vector3 currentRotation = transform.localEulerAngles;
        _currentYaw = currentRotation.y;
        _currentPitch = currentRotation.x;

        // Initial state sync
        SetMovementActive(canMove);
    }

    void Update()
    {
        // Only run rotation logic if movement is enabled
        if (canMove)
        {
            HandleRotation();
        }

        // Interaction logic
        HandleInteraction();
    }

    public void SetMovementActive(bool active)
    {
        canMove = active;

        // Manage the cursor state based on whether we are looking around
        if (active)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleRotation()
    {
        _currentYaw += Input.GetAxis("Mouse X") * sensitivity;
        _currentPitch -= Input.GetAxis("Mouse Y") * sensitivity;

        _currentYaw = Mathf.Clamp(_currentYaw, yawLimit.x, yawLimit.y);
        _currentPitch = Mathf.Clamp(_currentPitch, pitchLimit.x, pitchLimit.y);

        transform.localRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
    }

    private void HandleInteraction()
    {
        lastSeenObjectGO = null;



        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer) && !minigameActive)
        {
            if (hit.collider.TryGetComponent(out Interactable obj))
            {

                if(lastSeenObjectGO != hit.collider.gameObject)
                {
                    // If we are looking at a new object, disable the outline of the previous one
                    if (lastSeenObjectOutline != null)
                    {
                        lastSeenObjectOutline.enabled = false;
                    }

                    lastSeenObjectGO = hit.collider.gameObject;
                    lastSeenObjectOutline = hit.collider.GetComponent<Outline>();

                    if (lastSeenObjectOutline != null)
                        lastSeenObjectOutline.enabled = true; // Highlight the object
                }


                if (Input.GetMouseButtonDown(0))
                {
                    obj.Interact();
                }
            }
        }

        if (lastSeenObjectGO == null && lastSeenObjectOutline != null)
        {
            lastSeenObjectOutline.enabled = false; // Remove highlight
            lastSeenObjectOutline = null;
        }
    }
}