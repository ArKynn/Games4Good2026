using UnityEngine;

public class WorldCanvasBillboard : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool lockX = false;
    [SerializeField] private bool lockY = false;
    [SerializeField] private bool lockZ = false;
    [SerializeField] private bool flipForward = false;

    private Transform _mainCamTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            _mainCamTransform = Camera.main.transform;
        }
    }

    // LateUpdate is best for cameras to prevent "stuttering" 
    // after the camera has already moved in Update
    private void LateUpdate()
    {
        if (_mainCamTransform == null) return;

        // 1. Calculate the direction to face
        // We use the camera's rotation rather than LookAt to avoid "warping" at edges
        Vector3 targetRotation = _mainCamTransform.rotation.eulerAngles;

        // 2. Lock axes if needed (e.g., if you don't want it tilting up/down)
        if (lockX) targetRotation.x = transform.rotation.eulerAngles.x;
        if (lockY) targetRotation.y = transform.rotation.eulerAngles.y;
        if (lockZ) targetRotation.z = transform.rotation.eulerAngles.z;

        // 3. Apply rotation
        transform.rotation = Quaternion.Euler(targetRotation);

        // 4. Flip if the UI is showing backwards (common with some Canvas setups)
        if (flipForward)
        {
            transform.Rotate(0, 180, 0);
        }
    }
}