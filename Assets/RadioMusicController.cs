using UnityEngine;
using System.Collections;

public class RadioMusicController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] tracks;

    [Header("Timing")]
    [SerializeField] private float delayBetweenTracks = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private int currentIndex = 0;
    private Coroutine playRoutine;
    private float originalVolume;

    private bool isRadioOn = true;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        originalVolume = audioSource.volume;
    }

    private void Start()
    {
        if (tracks.Length > 0)
            PlayTrack(0);
    }

    // 🔘 TOGGLE POWER
    public void ToggleRadio()
    {
        if (isRadioOn)
        {
            // Turn OFF
            isRadioOn = false;

            if (playRoutine != null)
                StopCoroutine(playRoutine);

            StartCoroutine(FadeOutAndStop());
        }
        else
        {
            // Turn ON
            isRadioOn = true;

            if (tracks.Length > 0)
                PlayTrack(currentIndex);
        }
    }

    public void PlayTrack(int index)
    {
        if (!isRadioOn || tracks.Length == 0) return;

        currentIndex = (index + tracks.Length) % tracks.Length;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayWithFade());
    }

    private IEnumerator PlayWithFade()
    {
        // 🔻 Fade OUT current
        float t = 0f;
        float startVolume = audioSource.volume;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();

        // ⏸ Delay
        yield return new WaitForSeconds(delayBetweenTracks);

        if (!isRadioOn) yield break;

        // ▶ Play new track
        audioSource.clip = tracks[currentIndex];
        audioSource.Play();

        // 🔺 Fade IN
        t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, originalVolume, t / fadeInDuration);
            yield return null;
        }

        audioSource.volume = originalVolume;
    }

    private IEnumerator FadeOutAndStop()
    {
        float t = 0f;
        float startVolume = audioSource.volume;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    public void NextTrack()
    {
        if (!isRadioOn) return;
        PlayTrack(currentIndex + 1);
    }

    public void PreviousTrack()
    {
        if (!isRadioOn) return;
        PlayTrack(currentIndex - 1);
    }
}