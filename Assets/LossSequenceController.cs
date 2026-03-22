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
    [SerializeField] private Material irisMaterial;
    [SerializeField] private float irisDuration = 0.6f;
    [SerializeField] private Ease irisEase = Ease.InOutQuart;

    [Header("Screen Material Settings")]
    [SerializeField] private MeshRenderer screenRenderer;
    [SerializeField] private Material lossMaterial;

    [Header("Objects To Activate")]
    [SerializeField] private GameObject objectToActivate;

    [Header("Timing")]
    [SerializeField] private float waitAtLossPos = 2.0f;
    [SerializeField] private float waitAtScreenPos = 3.0f;
    [SerializeField] private float activationDelay = 0.4f; // 🔥 NEW

    [Header("Events")]
    public UnityEvent onSequenceComplete;

    private static readonly int MaskScaleID = Shader.PropertyToID("_MaskScale");

    [SerializeField] private FirstPersonViewport fpController;

    private void Start()
    {
        if (irisMaterial != null)
        {
            irisMaterial.SetFloat(MaskScaleID, 1f);
        }

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartLossSequence();
        }
    }

    public void StartLossSequence()
    {
        fpController.enabled = false;

        mainCamera.DOKill();

        irisMaterial.SetFloat(MaskScaleID, 1f);

        Sequence lossSeq = DOTween.Sequence();

        // 1. Move to Loss Position
        lossSeq.Append(mainCamera.DOMove(lossPosition.position, 1f).SetEase(Ease.OutSine));
        lossSeq.Join(mainCamera.DORotateQuaternion(lossPosition.rotation, 1f).SetEase(Ease.OutSine));
        lossSeq.AppendInterval(waitAtLossPos);

        // 2. Iris Out (Close)
        lossSeq.Append(DOTween.To(
            () => irisMaterial.GetFloat(MaskScaleID),
            x => irisMaterial.SetFloat(MaskScaleID, x),
            300f,
            irisDuration
        ).SetEase(irisEase));

        // 3. Teleport + change screen
        lossSeq.AppendCallback(() =>
        {
            mainCamera.position = screenPosition.position;
            mainCamera.rotation = screenPosition.rotation;

            if (screenRenderer != null && lossMaterial != null)
            {
                screenRenderer.material = lossMaterial;
            }
        });

        // 🎭 Dramatic delay
        lossSeq.AppendInterval(activationDelay);

        // 4. Activate object AFTER delay
        lossSeq.AppendCallback(() =>
        {
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
        });

        // 5. Iris In (Open)
        lossSeq.Append(DOTween.To(
            () => irisMaterial.GetFloat(MaskScaleID),
            x => irisMaterial.SetFloat(MaskScaleID, x),
            2f,
            irisDuration
        ).SetEase(irisEase));

        lossSeq.AppendInterval(waitAtScreenPos);

        // 6. Move to Reset Position
        lossSeq.Append(mainCamera.DOMove(resetPosition.position, 6f).SetEase(Ease.InOutExpo));
        lossSeq.Join(mainCamera.DORotateQuaternion(resetPosition.rotation, 6f).SetEase(Ease.InOutExpo));

        // 7. Final Event
        lossSeq.OnComplete(() => onSequenceComplete?.Invoke());
    }
}