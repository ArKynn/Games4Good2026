using UnityEngine;

public class InteractSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactClip;
    [SerializeField] private float volume = 1f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayInteractSound()
    {
        if (audioSource == null || interactClip == null)
            return;

        audioSource.PlayOneShot(interactClip, volume);
    }
}