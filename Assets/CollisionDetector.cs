using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // 1. Get the specific Collider component hit
        Collider hitCollider = collision.collider;

        // 2. Get the specific GameObject that collider belongs to
        GameObject exactHitObject = hitCollider.gameObject;

        // 3. Log both for comparison
        Debug.Log($"<color=cyan>RB Parent:</color> {collision.gameObject.name} | " +
                  $"<color=yellow>Exact Collider:</color> {exactHitObject.name}");


    }

    private void OnCollisionExit(Collision collision)
    {
        // Use collision.collider here as well to know which specific part stopped touching
        Debug.Log($"Stopped hitting: {collision.collider.gameObject.name}");
    }
}