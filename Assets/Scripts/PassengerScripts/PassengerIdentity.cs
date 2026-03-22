using System.Collections.Generic;
using UnityEngine;

public class PassengerIdentities : MonoBehaviour
{
    [SerializeField] private List<GameObject> PassengerPrefabs;
    [SerializeField] private List<Sprite> PassengerPassportPhotos;

    public Sprite GetPhoto(bool isCorrect, int prefabIndex = -1, GameObject prefab = null)
    {
        return isCorrect ? GetCorrectPhoto(prefabIndex, prefab) : GetRandomIncorrectPhoto(prefabIndex, prefab);
    }
    
    private Sprite GetCorrectPhoto(int prefabIndex = -1, GameObject prefab = null)
    {
        if(prefab != null && PassengerPrefabs.Contains(prefab)) prefabIndex = PassengerPrefabs.IndexOf(prefab);
        if(prefabIndex == -1) return null;
        return PassengerPassportPhotos[prefabIndex];
    }
    
    private Sprite GetRandomIncorrectPhoto(int prefabIndex = -1, GameObject prefab = null)
    {
        if(prefab != null && PassengerPrefabs.Contains(prefab)) prefabIndex = PassengerPrefabs.IndexOf(prefab);
        if(prefabIndex == -1) return null;
        
        var temp = Random.Range(0, PassengerPrefabs.Count-1);
        if (temp == prefabIndex) temp++;
        return PassengerPassportPhotos[temp];
    }
    
    public GameObject GetRandomIdentity()
    {
        return PassengerPrefabs[Random.Range(0, PassengerPrefabs.Count)];
    }
}
