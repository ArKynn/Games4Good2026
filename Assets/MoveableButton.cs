using UnityEngine;
using DG.Tweening;

public class MoveableButton : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0, -0.05f, 0);
    [SerializeField] private float pressDuration = 0.1f;
    [SerializeField] private float releaseDuration = 0.1f;
    [SerializeField] private Ease pressEase = Ease.InQuad;
    [SerializeField] private Ease releaseEase = Ease.OutQuad;

    [Header("Input Settings")]
    [SerializeField] private bool usesKey = false;
    // Just type the key name here (e.g., "w", "space", "f")
    [SerializeField] private string keyName = "w";

    private KeyCode _calculatedKey;
    private Vector3 _startLocalPos;
    private bool _isAnimating = false;

    void Awake()
    {
        _startLocalPos = transform.localPosition;

        // Convert the string to a KeyCode once at the start
        if (usesKey)
        {
            ParseKey();
        }
    }

    private void ParseKey()
    {
        string processedName = keyName.ToLower().Trim();

        // --- SHORTHAND CONVERSIONS ---

        // 1. Handle Numpad (e.g., "n1" or "num1" -> "Keypad1")
        if (processedName.StartsWith("n") && processedName.Length > 1 && char.IsDigit(processedName[processedName.Length - 1]))
        {
            // Take the last character (the digit) and attach "Keypad"
            processedName = "Keypad" + processedName[processedName.Length - 1];
        }
        // 2. Handle Top Row Numbers (e.g., "1" -> "Alpha1")
        else if (processedName.Length == 1 && char.IsDigit(processedName[0]))
        {
            processedName = "Alpha" + processedName;
        }

        // 3. Common Fixes
        if (processedName == "enter") processedName = "return";
        if (processedName == "esc") processedName = "escape";
        // -----------------------------

        if (System.Enum.TryParse(processedName, true, out KeyCode result))
        {
            _calculatedKey = result;
        }
        else
        {
            usesKey = false;
        }
    }

    private void Update()
    {
        if (usesKey && Input.GetKeyDown(_calculatedKey))
        {
            Press();
        }
    }

    public void Press()
    {
        if (_isAnimating) return;

        _isAnimating = true;

        Sequence buttonSeq = DOTween.Sequence();
        buttonSeq.Append(transform.DOLocalMove(_startLocalPos + moveOffset, pressDuration).SetEase(pressEase));
        buttonSeq.Append(transform.DOLocalMove(_startLocalPos, releaseDuration).SetEase(releaseEase));
        buttonSeq.OnComplete(() => _isAnimating = false);
    }
}