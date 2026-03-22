using UnityEngine;
using DG.Tweening;

public class CameraMover : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private Ease transitionEase = Ease.InOutSine;

    private Transform _mainCamTransform;

    private void Awake()
    {
        // Cache the transform for better performance
        if (Camera.main != null)
        {
            _mainCamTransform = Camera.main.transform;
        }
    }

    /// <summary>
    /// Moves the camera to the target position regardless of Time.timeScale.
    /// </summary>
    public void MoveCameraToTarget()
    {
        if (targetTransform == null || _mainCamTransform == null)
        {
            Debug.LogWarning("Missing Target Transform or Main Camera!");
            return;
        }

        // 1. Stop any current camera tweens immediately
        _mainCamTransform.DOKill();

        // 2. Move Position
        _mainCamTransform.DOMove(targetTransform.position, transitionDuration)
            .SetEase(transitionEase)
            .SetUpdate(true); // <--- This allows movement at TimeScale 0

        // 3. Rotate to Match
        _mainCamTransform.DORotateQuaternion(targetTransform.rotation, transitionDuration)
            .SetEase(transitionEase)
            .SetUpdate(true); // <--- This allows rotation at TimeScale 0
    }
}