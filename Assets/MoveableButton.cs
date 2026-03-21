using UnityEngine;
using DG.Tweening; // Uses your existing DOTween setup

public class MoveableButton : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0, -0.05f, 0);
    [SerializeField] private float pressDuration = 0.1f; // Time to go down
    [SerializeField] private float releaseDuration = 0.1f; // Time to come back up
    [SerializeField] private Ease pressEase = Ease.InQuad;
    [SerializeField] private Ease releaseEase = Ease.OutQuad;

    private Vector3 _startLocalPos;
    private bool _isAnimating = false;

    void Awake()
    {
        // Store the starting position relative to its parent
        _startLocalPos = transform.localPosition;
    }

    /// <summary>
    /// Call this from your Interactable.Interact() method
    /// </summary>
    public void Press()
    {
        // Prevent overlapping animations if the player spams the button
        if (_isAnimating) return;

        _isAnimating = true;

        // Create a sequence: Move to offset, then move back to start
        Sequence buttonSeq = DOTween.Sequence();

        buttonSeq.Append(transform.DOLocalMove(_startLocalPos + moveOffset, pressDuration).SetEase(pressEase));
        buttonSeq.Append(transform.DOLocalMove(_startLocalPos, releaseDuration).SetEase(releaseEase));

        buttonSeq.OnComplete(() => _isAnimating = false);
    }
}