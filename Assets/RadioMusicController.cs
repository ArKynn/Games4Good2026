using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadioMusicController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] tracks;

    [Header("Playback Mode")]
    [SerializeField] private bool randomPlayback = true;
    [SerializeField] private bool avoidImmediateRepeat = true;

    [Header("Timing")]
    [SerializeField] private float delayBetweenTracks = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float fadeInDuration = 0.5f;

    [Header("Power Button Visual")]
    [SerializeField] private Renderer buttonRenderer;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    private int currentIndex = -1;
    private Coroutine playRoutine;
    private float originalVolume;
    private bool isRadioOn = true;

    private readonly List<int> playedHistory = new List<int>();
    private int historyPosition = -1;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        originalVolume = audioSource.volume;
    }

    private void Start()
    {
        UpdateButtonVisual();

        if (tracks.Length > 0 && isRadioOn)
        {
            if (randomPlayback)
                PlayRandomTrack(addToHistory: true);
            else
                PlayTrackByIndex(0, addToHistory: true);
        }
    }

    private void Update()
    {
        if (!isRadioOn)
            return;

        if (tracks == null || tracks.Length == 0)
            return;

        if (isTransitioning)
            return;

        // Song finished naturally -> automatically play next
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            if (randomPlayback)
                PlayRandomTrack(addToHistory: true);
            else
                PlayTrackByIndex(currentIndex + 1, addToHistory: true);
        }
    }

    public void ToggleRadio()
    {
        if (isRadioOn)
        {
            isRadioOn = false;

            if (playRoutine != null)
                StopCoroutine(playRoutine);

            StartCoroutine(FadeOutAndStop());
        }
        else
        {
            isRadioOn = true;

            if (tracks.Length == 0)
            {
                UpdateButtonVisual();
                return;
            }

            if (currentIndex >= 0 && currentIndex < tracks.Length)
                PlayTrackByIndex(currentIndex, addToHistory: false);
            else if (randomPlayback)
                PlayRandomTrack(addToHistory: true);
            else
                PlayTrackByIndex(0, addToHistory: true);
        }

        UpdateButtonVisual();
    }

    public void NextTrack()
    {
        if (!isRadioOn || tracks.Length == 0)
            return;

        if (historyPosition >= 0 && historyPosition < playedHistory.Count - 1)
        {
            historyPosition++;
            PlayTrackByIndex(playedHistory[historyPosition], addToHistory: false);
            return;
        }

        if (randomPlayback)
            PlayRandomTrack(addToHistory: true);
        else
            PlayTrackByIndex(currentIndex + 1, addToHistory: true);
    }

    public void PreviousTrack()
    {
        if (!isRadioOn || tracks.Length == 0)
            return;

        if (historyPosition > 0)
        {
            historyPosition--;
            PlayTrackByIndex(playedHistory[historyPosition], addToHistory: false);
        }
    }

    private void PlayRandomTrack(bool addToHistory)
    {
        if (tracks.Length == 0)
            return;

        int nextIndex;

        if (tracks.Length == 1)
        {
            nextIndex = 0;
        }
        else
        {
            do
            {
                nextIndex = Random.Range(0, tracks.Length);
            }
            while (avoidImmediateRepeat && nextIndex == currentIndex);
        }

        PlayTrackByIndex(nextIndex, addToHistory);
    }

    private void PlayTrackByIndex(int index, bool addToHistory)
    {
        if (!isRadioOn || tracks.Length == 0)
            return;

        currentIndex = (index + tracks.Length) % tracks.Length;

        if (addToHistory)
        {
            if (historyPosition < playedHistory.Count - 1)
            {
                playedHistory.RemoveRange(historyPosition + 1, playedHistory.Count - historyPosition - 1);
            }

            playedHistory.Add(currentIndex);
            historyPosition = playedHistory.Count - 1;
        }

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayWithFade(currentIndex));
    }

    private IEnumerator PlayWithFade(int trackIndex)
    {
        isTransitioning = true;

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

        yield return new WaitForSeconds(delayBetweenTracks);

        if (!isRadioOn)
        {
            isTransitioning = false;
            yield break;
        }

        audioSource.clip = tracks[trackIndex];
        audioSource.Play();

        t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, originalVolume, t / fadeInDuration);
            yield return null;
        }

        audioSource.volume = originalVolume;
        isTransitioning = false;
    }

    private IEnumerator FadeOutAndStop()
    {
        isTransitioning = true;

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
        isTransitioning = false;
    }

    private void UpdateButtonVisual()
    {
        if (buttonRenderer != null && onMaterial != null && offMaterial != null)
        {
            buttonRenderer.material = isRadioOn ? onMaterial : offMaterial;
        }
    }
}