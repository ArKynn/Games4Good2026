using UnityEngine;
using UnityEngine.Events;

public class LayerCollisionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    public UnityEvent onLayerHitGood;
    public UnityEvent onLayerHitBad;

    [SerializeField] private GameObject celebrationEffect;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object's layer is in our LayerMask
        // (1 << layer) converts the layer index to a bitmask for comparison
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            SuitcaseObject suitcase = collision.gameObject.GetComponent<SuitcaseObject>();
            if(suitcase.wrong)
            {
                Instantiate(celebrationEffect, collision.contacts[0].point, Quaternion.identity);
                onLayerHitGood.Invoke();
            }
            else
            {
                onLayerHitBad.Invoke();
            }

            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object's layer is in our LayerMask
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            SuitcaseObject suitcase = other.gameObject.GetComponent<SuitcaseObject>();
            if (suitcase.wrong)
            {
                Instantiate(celebrationEffect, other.transform.position, Quaternion.identity);
                onLayerHitGood.Invoke();
            }
            else
            {
                onLayerHitBad.Invoke();
            }
            Destroy(other.gameObject);
        }
    }
}