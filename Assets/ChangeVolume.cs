using UnityEngine;

public class ChangeVolume : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void VolumeDown()
    {
        if (audioSource.volume > 0)
        {
            audioSource.volume -= 0.1f;
        }
    }

    public void VolumeUp()
    {
        if (audioSource.volume < 1)
        {
            audioSource.volume += 0.1f;
        }
    }
}
