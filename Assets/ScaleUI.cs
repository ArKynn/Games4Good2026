using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class ScaleUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InQuad;

    private Vector3 _initialScale;
    private Tween _currentTween;
    private bool _isOpen = false;

    [SerializeField] private UnityEvent onOpen;
    [SerializeField] private UnityEvent onClose;

    private void Awake()
    {
        _initialScale = transform.localScale;

        // If the object starts disabled, ensure scale is 0 for the first pop-up
        if (!gameObject.activeSelf)
            transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        // Listen for Q key to close the UI if it's currently open
        if (Input.GetKeyDown(KeyCode.Q) && _isOpen)
        {
            ScaleDown();
        }
    }

    /// <summary>
    /// Freezes the game and scales the UI up.
    /// </summary>
    public void ScaleUp()
    {

        if(onOpen != null)
            onOpen.Invoke();

        _currentTween?.Kill();
        _isOpen = true;

        // 1. Freeze Game Time
        Time.timeScale = 0f;

        gameObject.SetActive(true);

        // 2. Animate while ignoring TimeScale
        _currentTween = transform.DOScale(_initialScale, duration)
            .SetEase(showEase)
            .SetUpdate(true);
    }

    /// <summary>
    /// Scales the UI down, resumes game time, and disables the object.
    /// </summary>
    public void ScaleDown()
    {
        _currentTween?.Kill();
        _isOpen = false;

        // 3. Animate down while still ignoring TimeScale
        _currentTween = transform.DOScale(Vector3.zero, duration)
            .SetEase(hideEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                // 4. Resume Game Time and Hide
                Time.timeScale = 1f;
                if (onClose != null)
                    onClose.Invoke();
                gameObject.SetActive(false);

            });
    }
}