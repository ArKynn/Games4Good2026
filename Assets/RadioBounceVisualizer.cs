using UnityEngine;

public class RadioBounceVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform targetVisual;

    [Header("Base Scale")]
    [SerializeField] private Vector3 baseScale = Vector3.one;

    [Header("Bounce Strength")]
    [SerializeField] private float minStrength = 0.02f;
    [SerializeField] private float maxStrength = 0.18f;
    [SerializeField] private float rampUpTime = 1.5f;

    [Header("Bass Analysis")]
    [SerializeField] private int sampleSize = 512;
    [SerializeField] private int lowFrequencyBins = 12;
    [SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
    [SerializeField] private float bassSensitivity = 55f;

    [Header("Bass Threshold")]
    [SerializeField] private float bassThreshold = 0.22f;
    [SerializeField] private float bassHardness = 3.2f;

    [Header("Pulse Motion")]
    [SerializeField] private float pulseSpeed = 14f;
    [SerializeField] private float pulseAmount = 0.15f;

    [Header("Shape")]
    [SerializeField] private float xzMultiplier = 1.0f;
    [SerializeField] private float yMultiplier = 1.35f;

    [Header("Smoothing")]
    [SerializeField] private float scaleLerpSpeed = 10f;
    [SerializeField] private float returnSpeed = 6f;

    private float[] spectrum;
    private Vector3 currentScale;
    private float songStartTime;
    private bool wasPlayingLastFrame;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (targetVisual == null)
            targetVisual = transform;

        spectrum = new float[sampleSize];
        baseScale = targetVisual.localScale;
        currentScale = baseScale;
    }

    private void Update()
    {
        if (audioSource == null)
            return;

        bool isPlaying = audioSource.isPlaying;

        if (isPlaying && !wasPlayingLastFrame)
            songStartTime = Time.time;

        wasPlayingLastFrame = isPlaying;

        if (!isPlaying)
        {
            currentScale = Vector3.Lerp(currentScale, baseScale, returnSpeed * Time.deltaTime);
            targetVisual.localScale = currentScale;
            return;
        }

        audioSource.GetSpectrumData(spectrum, 0, fftWindow);

        float lowSum = 0f;
        int bins = Mathf.Min(lowFrequencyBins, spectrum.Length);

        for (int i = 0; i < bins; i++)
            lowSum += spectrum[i];

        float bassValue = lowSum / bins;

        float ramp = Mathf.Clamp01((Time.time - songStartTime) / rampUpTime);
        float strength = Mathf.Lerp(minStrength, maxStrength, ramp);

        float normalizedBass = Mathf.Clamp01(bassValue * bassSensitivity);

        // Weak bass barely moves
        float thresholdedBass = Mathf.InverseLerp(bassThreshold, 1f, normalizedBass);
        thresholdedBass = Mathf.Pow(thresholdedBass, bassHardness);

        // Tiny pulse so loud sections still "breathe"
        float pulseWave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float pulsedBass = thresholdedBass * Mathf.Lerp(1f - pulseAmount, 1f, pulseWave);

        float pulse = pulsedBass * strength;

        Vector3 targetScale = baseScale + new Vector3(
            pulse * xzMultiplier,
            pulse * yMultiplier,
            pulse * xzMultiplier
        );

        currentScale = Vector3.Lerp(currentScale, targetScale, scaleLerpSpeed * Time.deltaTime);
        targetVisual.localScale = currentScale;
    }

    private void OnDisable()
    {
        if (targetVisual != null)
            targetVisual.localScale = baseScale;
    }

    private void OnDestroy()
    {
        if (targetVisual != null)
            targetVisual.localScale = baseScale;
    }
}