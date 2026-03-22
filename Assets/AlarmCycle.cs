using UnityEngine;
using DG.Tweening;

public class AlarmCycle : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private MeshRenderer targetRenderer;
    [SerializeField] private Material materialA; // Normal
    [SerializeField] private Material materialB; // Alarm

    [Header("Light Settings")]
    [SerializeField] private Light alarmLight;

    [Header("Animation Settings")]
    [SerializeField] private int loopCount = 3; // Number of times to "pulse"
    [SerializeField] private float cycleDuration = 0.5f;
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private Ease scaleEase = Ease.InOutSine;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip alarmSound;

    private Vector3 _initialScale;
    private Sequence _alarmSequence;

    void Awake()
    {
        _initialScale = transform.localScale;
    }

    private void Start()
    {

    }

    /// <summary>
    /// Starts the alarm for the specific number of loops defined in the Inspector.
    /// </summary>
    public void StartAlarm()
    {
        // Kill existing sequence if user triggers it again while running
        _alarmSequence?.Kill();
        ResetToNormal();

        // Audio logic
        if (audioSource != null && alarmSound != null)
        {
            // We play it as a one-shot or loop based on duration, 
            // but for a fixed loop count, PlayOneShot is often cleaner.
            audioSource.PlayOneShot(alarmSound);
        }

        _alarmSequence = DOTween.Sequence();

        // The animation loop
        for (int i = 0; i < loopCount; i++)
        {
            // 1. Scale Up + Turn ON
            _alarmSequence.AppendCallback(() => SetAlarmVisuals(true));
            _alarmSequence.Append(transform.DOScale(_initialScale * scaleMultiplier, cycleDuration).SetEase(scaleEase));

            // 2. Scale Down + Turn OFF
            _alarmSequence.AppendCallback(() => SetAlarmVisuals(false));
            _alarmSequence.Append(transform.DOScale(_initialScale, cycleDuration).SetEase(scaleEase));
        }

        // Final cleanup after all loops finish
        _alarmSequence.OnComplete(ResetToNormal);
    }

    private void SetAlarmVisuals(bool isOn)
    {
        if (targetRenderer) targetRenderer.material = isOn ? materialB : materialA;
        if (alarmLight) alarmLight.enabled = isOn;
    }

    private void ResetToNormal()
    {
        transform.localScale = _initialScale;
        SetAlarmVisuals(false);
    }

    private void OnDestroy()
    {
        _alarmSequence?.Kill();
    }
}