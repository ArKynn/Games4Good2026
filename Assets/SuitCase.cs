using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class SuitCase : MonoBehaviour
{

    [SerializeField] private Collider spawnBounds;
    [SerializeField] private Vector2 itemsToSpawn = new Vector2(4, 8);

    [SerializeField] private SuitcaseObject[] possibleItems;
    private List<SuitcaseObject> spawnedItems = new List<SuitcaseObject>();
    public int numberOfBadItems = 0;

    public bool CheckCase()
    {

        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            SuitcaseObject item = spawnedItems[i];
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        spawnedItems.Clear();

        if (numberOfBadItems > 0)
        {
            Debug.Log("Case failed! Bad items found.");
            return false;
        }
        else
        {
            Debug.Log("Case passed! No bad items found.");
            return true;
        }
    }

    public void SpawnSuitCase(Vector3 spawnPosition)
    {
        transform.position = spawnPosition + new Vector3(0, 25f, 0); // Adjust Y position to be above the ground

        transform.DOMoveY(spawnPosition.y, 1f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            SpawnItems();
        });
    }

    public void SpawnItems()
    {
        if (spawnBounds != null)
        {
            Vector3 boundsSize = spawnBounds.bounds.size;
            Vector3 boundsCenter = spawnBounds.bounds.center;
            int itemCount = Random.Range((int)itemsToSpawn.x, (int)itemsToSpawn.y + 1);
            for (int i = 0; i < itemCount; i++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(boundsCenter.x - boundsSize.x / 2, boundsCenter.x + boundsSize.x / 2),
                    Random.Range(boundsCenter.y - boundsSize.y / 2, boundsCenter.y + boundsSize.y / 2),
                    Random.Range(boundsCenter.z - boundsSize.z / 2, boundsCenter.z + boundsSize.z / 2)
                );

                // Instantiate your item prefab at randomPosition
                SuitcaseObject newItem = Instantiate(possibleItems[Random.Range(0, possibleItems.Length)], randomPosition, Quaternion.identity);

                newItem.transform.eulerAngles = new Vector3(90f, 0, 0); // Randomize rotation for variety
                spawnedItems.Add(newItem);
                if (newItem.wrong)
                {
                    numberOfBadItems++;
                }
            }
        }
    }
}
