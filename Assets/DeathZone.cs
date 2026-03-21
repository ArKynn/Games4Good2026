using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Try to find a Rigidbody (works for your items and suitcase)
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // 2. Move the object
            rb.transform.position = respawnPoint.position;
            rb.transform.rotation = respawnPoint.rotation;

            // 3. IMPORTANT: Reset velocity so it doesn't keep falling/spinning
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            // If it's a simple object without physics, just move the transform
            other.transform.position = respawnPoint.position;
        }
    }
}