using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private LayerMask detectionLayer; // Set this to "Items" or "Suitcase" in Inspector

    [Header("Spawn Settings")]
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object's layer is included in our LayerMask
        if (((1 << other.gameObject.layer) & detectionLayer) != 0)
        {
            // 1. Try to find a Rigidbody
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // 2. Move and Reset Physics
                rb.transform.position = respawnPoint.position;
                rb.transform.rotation = respawnPoint.rotation;

                // Reset velocities so it doesn't "carry" the fall speed to the new spot
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // 3. Fallback for non-physics objects
                other.transform.position = respawnPoint.position;
                other.transform.rotation = respawnPoint.rotation;
            }

            Debug.Log($"<color=red>DeathZone:</color> Respawned {other.gameObject.name}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object's layer is included in our LayerMask
        if (((1 << collision.gameObject.layer) & detectionLayer) != 0)
        {
            // 1. Try to find a Rigidbody
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 2. Move and Reset Physics
                rb.transform.position = respawnPoint.position;
                rb.transform.rotation = respawnPoint.rotation;
                // Reset velocities so it doesn't "carry" the fall speed to the new spot
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // 3. Fallback for non-physics objects
                collision.transform.position = respawnPoint.position;
                collision.transform.rotation = respawnPoint.rotation;
            }
            Debug.Log($"<color=red>DeathZone:</color> Respawned {collision.gameObject.name}");
        }
    }
}