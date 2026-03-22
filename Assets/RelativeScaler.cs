using UnityEngine;
using DG.Tweening;

public class InspectorScaler : MonoBehaviour
{
    [Header("Scale Up Settings")]
    [SerializeField] private float upMultiplier = 1.2f;
    [SerializeField] private float upDuration = 0.3f;
    [SerializeField] private Ease upEase = Ease.OutBack;

    [Header("Scale Down Settings")]
    [SerializeField] private float downMultiplier = 1.0f; // Usually 1.0 to return to normal
    [SerializeField] private float downDuration = 0.2f;
    [SerializeField] private Ease downEase = Ease.InQuad;

    private Vector3 _initialScale;
    private Tween _currentTween;

    void Awake()
    {
        // Store the scale exactly as it is in the prefab/scene at start
        _initialScale = transform.localScale;
    }

    /// <summary>
    /// Triggers the Scale Up animation using Inspector values.
    /// </summary>
    public void ScaleUp()
    {
        _currentTween?.Kill();

        Vector3 targetScale = _initialScale * upMultiplier;
        _currentTween = transform.DOScale(targetScale, upDuration).SetEase(upEase);
    }

    /// <summary>
    /// Triggers the Scale Down animation using Inspector values.
    /// </summary>
    public void ScaleDown()
    {
        _currentTween?.Kill();

        Vector3 targetScale = _initialScale * downMultiplier;
        _currentTween = transform.DOScale(targetScale, downDuration).SetEase(downEase);
    }
}