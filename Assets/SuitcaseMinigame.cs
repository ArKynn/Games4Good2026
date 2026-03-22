using UnityEngine;
using DG.Tweening;
using System.Collections;

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
    [SerializeField] private int lidLayer;

    [Header("Scroll & Rotate Settings")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float minScrollDistance = 0.5f;
    [SerializeField] private float maxScrollDistance = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    
    private Rigidbody _grabbedRb;
    private float _dragDepth;
    private Vector3 _grabOffset;
    private bool _isCaseLead;
    private bool _isLid;
    private bool _isRotating;

    [SerializeField] private SuitCase suitCase;
    private SuitCase currentCase;
    public SuitCase CurrentCase => currentCase;
    [SerializeField] private Transform suitCaseSpawnPos;

    [Header("Puff Effect")]
    [SerializeField] private GameObject puffEffect;

    [Header("Puff Sound")]
    [SerializeField] private AudioSource puffAudioSource;
    [SerializeField] private AudioClip puffClip;
    [SerializeField] private float puffVolume = 1f;

    private SuitcaseController _suitcaseController;

    void Start()
    {
        _suitcaseController = FindFirstObjectByType<SuitcaseController>();

        if (puffAudioSource == null)
            puffAudioSource = GetComponent<AudioSource>();
    }

    public void ActivateGame(bool toggle)
    {
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
                    active = toggle;
                    instructionsUI.SetActive(true);
                    instructionsUI.transform.localScale = Vector3.zero;
                    instructionsUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                }
            });
            Camera.main.transform.DORotate(cameraPosition.eulerAngles, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            active = toggle;
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

    public void DecreaseBadItem()
    {
        if (currentCase != null)
        {
            currentCase.numberOfBadItems -= 1;
            if (currentCase.numberOfBadItems <= 0)
            {
                currentCase.numberOfBadItems = 0;
                Debug.Log("No more bad items");
            }
        }
    }

    public void CheckCurrentCase()
    {
        if (currentCase != null)
        {
            if (currentCase.CheckCase())
            {
                Debug.Log("Case passed! Proceeding to next case...");
            }
            else
            {
                Debug.Log("Case failed! Try again.");
                _suitcaseController.ErrorHandlingCase();
            }

            _suitcaseController.DoneInspectingCase();
            StartDespawnCase();
        }
    }

    public void StartDespawnCase()
    {
        if (puffEffect && currentCase != null)
        {
            currentCase.CheckCase();

            currentCase.transform.DOScale(Vector3.one * 0.1f, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                Vector3 puffPosition = currentCase.transform.position + new Vector3(0f, 0.25f, 0f);

                // Spawn puff effect
                Instantiate(puffEffect, puffPosition, Quaternion.identity);

                // Play puff / explosion sound
                if (puffAudioSource != null && puffClip != null)
                {
                    puffAudioSource.pitch = Random.Range(0.95f, 1.05f);
                    puffAudioSource.PlayOneShot(puffClip, puffVolume);
                }

                // Destroy old case
                Destroy(currentCase.gameObject);
            });
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && active)
        {
            ActivateGame(false);
        }

        if (!active) return;

        if (Input.GetMouseButtonDown(0)) TryGrab();
        if (Input.GetMouseButtonUp(0)) Release();

        if (_grabbedRb != null && !_isCaseLead && !_isLid)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _dragDepth = Mathf.Clamp(_dragDepth + (scroll * scrollSpeed), minScrollDistance, maxScrollDistance);
            }

            _isRotating = Input.GetMouseButton(1);
        }
    }

    public void SpawnSuitCase()
    {
        print("suit spawn");
        SuitCase spawnedCase = Instantiate(suitCase, suitCaseSpawnPos.position, Quaternion.identity);
        spawnedCase.transform.eulerAngles = new Vector3(0, 90f, 0);

        currentCase = spawnedCase;

        spawnedCase.GetComponentInChildren<Interactable>().onInteract.AddListener(() =>
        {
            ActivateGame(true);
        });

        spawnedCase.SpawnSuitCase(suitCaseSpawnPos.position);
    }

    void FixedUpdate()
    {
        if (active && _grabbedRb != null)
        {
            if (_isRotating)
                HandleRotation();
            else
                HandlePhysicsDrag();
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
                _isLid = hit.collider.gameObject.layer == lidLayer;

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
        _grabbedRb.linearVelocity = Vector3.zero;
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
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
            _isLid = false;
            _isRotating = false;
        }
    }
}