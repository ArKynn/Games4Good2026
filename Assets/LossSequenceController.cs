using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.InputSystem.XR;

public class LossSequenceController : MonoBehaviour
{
    [Header("Camera & Transforms")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform lossPosition;
    [SerializeField] private Transform screenPosition;
    [SerializeField] private Transform resetPosition;

    [Header("Shader Mask Settings")]
    [SerializeField] private Material irisMaterial; // The material with MaskScale
    [SerializeField] private float irisDuration = 0.6f;
    [SerializeField] private Ease irisEase = Ease.InOutQuart;

    [Header("Screen Material Settings")]
    [SerializeField] private MeshRenderer screenRenderer;
    [SerializeField] private Material lossMaterial;

    [Header("Timing")]
    [SerializeField] private float waitAtLossPos = 2.0f;
    [SerializeField] private float waitAtScreenPos = 3.0f;

    [Header("Events")]
    public UnityEvent onSequenceComplete;

    // Shader property ID for performance
    private static readonly int MaskScaleID = Shader.PropertyToID("_MaskScale");

    [SerializeField] private FirstPersonViewport fpController;


    private void Start()
    {
        // Ensure the iris material starts with the hole open
        if (irisMaterial != null)
        {
            irisMaterial.SetFloat(MaskScaleID, 1f);
        }
    }


    private void Update()
    {
        // For testing: Press L to start the loss sequence
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartLossSequence();
        }
    }

    public void StartLossSequence()
    {
        fpController.enabled = false;
        // Safety: Kill existing tweens
        mainCamera.DOKill();

        // Ensure material starts at 1 (Open)
        irisMaterial.SetFloat(MaskScaleID, 1f);

        Sequence lossSeq = DOTween.Sequence();

        // 1. Move to Loss Position & Wait
        lossSeq.Append(mainCamera.DOMove(lossPosition.position, 1f).SetEase(Ease.OutSine));
        lossSeq.Join(mainCamera.DORotateQuaternion(lossPosition.rotation, 1f).SetEase(Ease.OutSine));
        lossSeq.AppendInterval(waitAtLossPos);

        // 2. Iris Out (Close Hole: 1 -> 50)
        // Using DOTween.To to target the shader float
        lossSeq.Append(DOTween.To(() => irisMaterial.GetFloat(MaskScaleID),
            x => irisMaterial.SetFloat(MaskScaleID, x), 300f, irisDuration).SetEase(irisEase));

        // 3. Instant Teleport to Screen & Change Material (Hidden)
        lossSeq.AppendCallback(() =>
        {
            mainCamera.position = screenPosition.position;
            mainCamera.rotation = screenPosition.rotation;

            if (screenRenderer != null && lossMaterial != null)
            {
                screenRenderer.material = lossMaterial;
            }
        });

        // 4. Iris In (Open Hole: 50 -> 1) & Wait to see the screen
        lossSeq.Append(DOTween.To(() => irisMaterial.GetFloat(MaskScaleID),
            x => irisMaterial.SetFloat(MaskScaleID, x), 2f, irisDuration).SetEase(irisEase));

        lossSeq.AppendInterval(waitAtScreenPos);

        // 5. Final Move to Reset Position
        lossSeq.Append(mainCamera.DOMove(resetPosition.position, 6f).SetEase(Ease.InOutExpo));
        lossSeq.Join(mainCamera.DORotateQuaternion(resetPosition.rotation, 6f).SetEase(Ease.InOutExpo));

        // 6. Trigger the Final Event
        lossSeq.OnComplete(() => onSequenceComplete?.Invoke());
    }
}