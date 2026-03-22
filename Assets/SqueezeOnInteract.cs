using UnityEngine;
using DG.Tweening;
using System.Collections;

public class SqueezeOnInteract : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetVisual;

    [Header("Optional Mesh Objects To Hide")]
    [SerializeField] private GameObject[] visualObjectsToHide;

    [Header("Normal Press")]
    [SerializeField] private Vector3 normalPunch = new Vector3(0.12f, -0.18f, 0.12f);
    [SerializeField] private float normalDuration = 0.18f;
    [SerializeField] private int normalVibrato = 8;
    [SerializeField] [Range(0f, 1f)] private float normalElasticity = 0.8f;

    [Header("Fast Press Detection")]
    [SerializeField] private float maxGapBetweenFastPresses = 0.18f;
    [SerializeField] private int rapidPressesBeforeDeform = 3;

    [Header("Rapid Interact Trigger")]
    [SerializeField] private int interactionsNeeded = 8;

    [Header("Fast Spam Deformation")]
    [SerializeField] private Vector3 deformationPerFastPress = new Vector3(0.04f, -0.07f, 0.04f);
    [SerializeField] private Vector3 maxDeformation = new Vector3(0.35f, -0.55f, 0.35f);
    [SerializeField] private float deformTweenDuration = 0.05f;
    [SerializeField] private float resetAfterFailedSpam = 0.12f;

    [Header("Explosion Effect")]
    [SerializeField] private GameObject celebrationEffect;
    [SerializeField] private Transform effectSpawnPoint;

    [Header("Explosion Squeeze")]
    [SerializeField] private Vector3 finalPunch = new Vector3(0.35f, -0.5f, 0.35f);
    [SerializeField] private float finalPunchDuration = 0.16f;
    [SerializeField] private int finalVibrato = 10;
    [SerializeField] [Range(0f, 1f)] private float finalElasticity = 0.9f;

    [Header("Respawn From Sky")]
    [SerializeField] private float respawnHeight = 8f;
    [SerializeField] private float hiddenTime = 0.35f;
    [SerializeField] private float fallDuration = 0.6f;
    [SerializeField] private Ease fallEase = Ease.InQuad;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 currentDeformation = Vector3.zero;

    private int fastPressCount = 0;
    private int deformPhasePressCount = 0;
    private float lastInteractTime = -999f;

    private bool isRespawning = false;
    private bool isInDeformPhase = false;

    private Tween currentTween;
    private Tween resetTween;
    private Tween fallTween;
    private Coroutine comboFailRoutine;

    private Collider[] allColliders;
    private Renderer[] allRenderers;

    private void Awake()
    {
        if (targetVisual == null)
            targetVisual = transform;

        if (effectSpawnPoint == null)
            effectSpawnPoint = transform;

        originalScale = targetVisual.localScale;
        originalPosition = transform.position;

        allColliders = GetComponentsInChildren<Collider>(true);
        allRenderers = GetComponentsInChildren<Renderer>(true);
    }

    public void PlaySqueeze()
    {
        if (isRespawning)
            return;

        float timeSinceLast = Time.time - lastInteractTime;
        bool isFastEnough = timeSinceLast <= maxGapBetweenFastPresses;

        // First press or too slow: restart chain
        if (fastPressCount == 0 || !isFastEnough)
        {
            ResetComboState(false);

            fastPressCount = 1;
            lastInteractTime = Time.time;

            PlayNormalPunch();
            return;
        }

        // Still fast enough
        fastPressCount++;
        lastInteractTime = Time.time;

        if (comboFailRoutine != null)
        {
            StopCoroutine(comboFailRoutine);
            comboFailRoutine = null;
        }

        // Still in safe detection phase
        if (!isInDeformPhase)
        {
            if (fastPressCount >= rapidPressesBeforeDeform)
            {
                isInDeformPhase = true;
                deformPhasePressCount = rapidPressesBeforeDeform;

                AddFastDeformation();
                StartFailTimer();
            }

            return;
        }

        // Already deforming and counting
        deformPhasePressCount++;

        if (deformPhasePressCount >= interactionsNeeded)
        {
            TriggerExplosionAndRespawn();
            return;
        }

        AddFastDeformation();
        StartFailTimer();
    }

    private void PlayNormalPunch()
    {
        KillScaleTweens();

        targetVisual.localScale = originalScale;

        currentTween = targetVisual.DOPunchScale(
            normalPunch,
            normalDuration,
            normalVibrato,
            normalElasticity
        ).OnComplete(() =>
        {
            if (!isInDeformPhase && !isRespawning)
                targetVisual.localScale = originalScale;
        });
    }

    private void AddFastDeformation()
    {
        KillScaleTweens();

        currentDeformation += deformationPerFastPress;
        currentDeformation.x = Mathf.Min(currentDeformation.x, maxDeformation.x);
        currentDeformation.y = Mathf.Max(currentDeformation.y, maxDeformation.y);
        currentDeformation.z = Mathf.Min(currentDeformation.z, maxDeformation.z);

        Vector3 targetScale = originalScale + currentDeformation;
        currentTween = targetVisual.DOScale(targetScale, deformTweenDuration);
    }

    private void StartFailTimer()
    {
        if (comboFailRoutine != null)
            StopCoroutine(comboFailRoutine);

        comboFailRoutine = StartCoroutine(FailComboIfNoMoreFastPresses());
    }

    private IEnumerator FailComboIfNoMoreFastPresses()
    {
        yield return new WaitForSeconds(maxGapBetweenFastPresses);

        if (Time.time - lastInteractTime >= maxGapBetweenFastPresses)
        {
            ResetComboState(true);
        }

        comboFailRoutine = null;
    }

    private void ResetComboState(bool animateBack)
    {
        fastPressCount = 0;
        deformPhasePressCount = 0;
        isInDeformPhase = false;
        lastInteractTime = -999f;
        currentDeformation = Vector3.zero;

        if (comboFailRoutine != null)
        {
            StopCoroutine(comboFailRoutine);
            comboFailRoutine = null;
        }

        KillScaleTweens();

        if (animateBack)
            resetTween = targetVisual.DOScale(originalScale, resetAfterFailedSpam);
        else
            targetVisual.localScale = originalScale;
    }

    private void TriggerExplosionAndRespawn()
    {
        if (comboFailRoutine != null)
        {
            StopCoroutine(comboFailRoutine);
            comboFailRoutine = null;
        }

        isRespawning = true;
        fastPressCount = 0;
        deformPhasePressCount = 0;
        isInDeformPhase = false;
        lastInteractTime = -999f;
        currentDeformation = Vector3.zero;

        KillScaleTweens();

        // Do NOT snap back to original scale here.
        // Start the final punch from the current deformed state.
        currentTween = targetVisual.DOPunchScale(
            finalPunch,
            finalPunchDuration,
            finalVibrato,
            finalElasticity
        ).OnComplete(() =>
        {
            targetVisual.localScale = originalScale;

            if (celebrationEffect != null)
                Instantiate(celebrationEffect, effectSpawnPoint.position, Quaternion.identity);

            StartCoroutine(RespawnRoutine());
        });
    }

    private IEnumerator RespawnRoutine()
    {
        SetInteractionEnabled(false);
        HideVisuals();

        yield return new WaitForSeconds(hiddenTime);

        transform.position = originalPosition + Vector3.up * respawnHeight;
        ShowVisuals();

        fallTween = transform.DOMove(originalPosition, fallDuration).SetEase(fallEase);
        yield return fallTween.WaitForCompletion();

        transform.position = originalPosition;
        targetVisual.localScale = originalScale;
        isRespawning = false;

        SetInteractionEnabled(true);
    }

    private void HideVisuals()
    {
        if (visualObjectsToHide != null && visualObjectsToHide.Length > 0)
        {
            foreach (GameObject obj in visualObjectsToHide)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
        else
        {
            foreach (Renderer rend in allRenderers)
            {
                if (rend != null)
                    rend.enabled = false;
            }
        }
    }

    private void ShowVisuals()
    {
        if (visualObjectsToHide != null && visualObjectsToHide.Length > 0)
        {
            foreach (GameObject obj in visualObjectsToHide)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
        else
        {
            foreach (Renderer rend in allRenderers)
            {
                if (rend != null)
                    rend.enabled = true;
            }
        }
    }

    private void SetInteractionEnabled(bool enabled)
    {
        foreach (Collider col in allColliders)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }

    private void KillScaleTweens()
    {
        if (currentTween != null && currentTween.IsActive())
            currentTween.Kill();

        if (resetTween != null && resetTween.IsActive())
            resetTween.Kill();

        targetVisual.DOKill();
    }

    private void OnDisable()
    {
        FullReset();
    }

    private void OnDestroy()
    {
        FullReset();
    }

    private void FullReset()
    {
        if (comboFailRoutine != null)
        {
            StopCoroutine(comboFailRoutine);
            comboFailRoutine = null;
        }

        KillScaleTweens();

        if (fallTween != null && fallTween.IsActive())
            fallTween.Kill();

        targetVisual.localScale = originalScale;
        transform.position = originalPosition;

        fastPressCount = 0;
        deformPhasePressCount = 0;
        isInDeformPhase = false;
        lastInteractTime = -999f;
        currentDeformation = Vector3.zero;
        isRespawning = false;

        ShowVisuals();
        SetInteractionEnabled(true);
    }
}