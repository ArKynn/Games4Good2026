using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ComputerNotifications : MonoBehaviour
{
    [System.Serializable]
    public struct NotificationSet
    {
        public string name;
        public Sprite promptSprite;
        public Sprite correctOptionSprite;
        public Sprite wrongOptionSprite;
    }

    [Header("UI References")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Image promptDisplay;
    [SerializeField] private Image timerBar;

    [Header("Feedback Icons")]
    [SerializeField] private GameObject correctFeedbackObj;
    [SerializeField] private GameObject wrongFeedbackObj;
    [SerializeField] private float feedbackDisplayTime = 1.2f;

    [Header("Buttons")]
    [SerializeField] private RectTransform buttonARect;
    [SerializeField] private Image buttonAChildIcon;
    [SerializeField] private RectTransform buttonBRect;
    [SerializeField] private Image buttonBChildIcon;

    [Header("Settings")]
    [SerializeField] private List<NotificationSet> library;
    [SerializeField] private float interval = 10f;
    [SerializeField] private float duration = 5f;

    [Header("Events")]
    public UnityEvent onCorrect;
    public UnityEvent onWrong;
    public UnityEvent onNewNotification;

    private Vector3 _panelInitialScale;
    private Vector3 _buttonAInitialScale;
    private Vector3 _buttonBInitialScale;
    private Vector3 _correctIconInitialScale;
    private Vector3 _wrongIconInitialScale;

    private int _correctButtonIndex;
    private bool _isActive;
    private bool _isProcessingFeedback;
    private float _timer;

    void Awake()
    {
        _panelInitialScale = panelRect.localScale;
        _buttonAInitialScale = buttonARect.localScale;
        _buttonBInitialScale = buttonBRect.localScale;
        _correctIconInitialScale = correctFeedbackObj.transform.localScale;
        _wrongIconInitialScale = wrongFeedbackObj.transform.localScale;

        ResetUI();
    }

    void ResetUI()
    {
        panelRect.localScale = Vector3.zero;
        panelRect.gameObject.SetActive(false);

        buttonARect.localScale = Vector3.zero;
        buttonBRect.localScale = Vector3.zero;

        correctFeedbackObj.transform.localScale = Vector3.zero;
        correctFeedbackObj.SetActive(false);

        wrongFeedbackObj.transform.localScale = Vector3.zero;
        wrongFeedbackObj.SetActive(false);
    }

    public void StartNotifications()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            if (library.Count > 0)
            {
                yield return StartCoroutine(ShowNotificationAndWait());
            }
        }
    }

    IEnumerator ShowNotificationAndWait()
    {
        onNewNotification?.Invoke();
        _isActive = true;
        _isProcessingFeedback = false;
        _timer = duration;

        NotificationSet set = library[Random.Range(0, library.Count)];
        promptDisplay.sprite = set.promptSprite;

        _correctButtonIndex = Random.Range(0, 2);

        buttonAChildIcon.sprite = (_correctButtonIndex == 0) ? set.correctOptionSprite : set.wrongOptionSprite;
        buttonBChildIcon.sprite = (_correctButtonIndex == 1) ? set.correctOptionSprite : set.wrongOptionSprite;

        panelRect.gameObject.SetActive(true);

        Sequence appearSeq = DOTween.Sequence();
        appearSeq.Append(panelRect.DOScale(_panelInitialScale, 0.4f).SetEase(Ease.OutBack));
        appearSeq.Join(buttonARect.DOScale(_buttonAInitialScale, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f));
        appearSeq.Join(buttonBRect.DOScale(_buttonBInitialScale, 0.3f).SetEase(Ease.OutBack).SetDelay(0.15f));

        while (_isActive)
        {
            yield return null;
        }
    }

    void Update()
    {
        if (!_isActive || _isProcessingFeedback) return;

        _timer -= Time.deltaTime;
        timerBar.fillAmount = _timer / duration;

        if (_timer <= 0) HandleChoice(-1);
    }

    public void ClickButtonA() => HandleChoice(0);
    public void ClickButtonB() => HandleChoice(1);

    private void HandleChoice(int index)
    {
        if (!_isActive || _isProcessingFeedback) return;

        _isProcessingFeedback = true;

        bool isCorrect = (index == _correctButtonIndex);

        if (isCorrect) onCorrect?.Invoke();
        else onWrong?.Invoke();

        StartCoroutine(FeedbackSequence(isCorrect));
    }

    IEnumerator FeedbackSequence(bool correct)
    {
        GameObject iconObj = correct ? correctFeedbackObj : wrongFeedbackObj;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        Vector3 targetScale = correct ? _correctIconInitialScale : _wrongIconInitialScale;

        // 1. Set Random Position using your specific bounds
        iconRect.anchoredPosition = new Vector2(Random.Range(-500f, 500f), Random.Range(-200f, 200f));

        // 2. Enable and Scale Up
        iconObj.SetActive(true);
        iconObj.transform.localScale = Vector3.zero;

        // 3. Start the Shake (Looping)
        // We use -1 for loops to keep it shaking until we manually kill it
        Tween shakeTween = iconRect.DOShakePosition(feedbackDisplayTime, 15f, 10, 90, false, false).SetLoops(-1);

        yield return iconObj.transform.DOScale(targetScale, 0.4f).SetEase(Ease.OutBack).WaitForCompletion();

        // 4. Wait for the display time
        yield return new WaitForSeconds(feedbackDisplayTime);

        // 5. Kill shake and Scale Down
        shakeTween.Kill();
        yield return iconObj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).WaitForCompletion();

        iconObj.SetActive(false);
        ClosePanel();
    }

    private void ClosePanel()
    {
        panelRect.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => {
            panelRect.gameObject.SetActive(false);
            ResetUI();
            _isActive = false;
        });
    }
}