using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class MenuController : MonoBehaviour
{
    [Header("Play Animation Settings")]
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private float shrinkDuration = 0.5f;
    [SerializeField] private Ease shrinkEase = Ease.InBack;

    [Header("Events")]
    public UnityEvent onPlayConfirmed;

    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    /// <summary>
    /// Descales the menu panel and then triggers the Play event.
    /// </summary>
    public void Play()
    {
        if (menuPanel == null)
        {
            Debug.LogWarning("Menu Panel RectTransform is not assigned!");
            onPlayConfirmed?.Invoke();
            return;
        }

        if(playButton != null) playButton.interactable = false;
        if (quitButton != null) quitButton.interactable = false;

        // Scale down to zero
        menuPanel.DOScale(Vector3.zero, shrinkDuration)
            .SetEase(shrinkEase)
            .OnComplete(() =>
            {
                // Trigger the logic (e.g., starting the game or loading a scene)
                // only after the animation finishes.
                onPlayConfirmed?.Invoke();

                // Optional: Deactivate the panel so it doesn't block raycasts
                menuPanel.gameObject.SetActive(false);
            });
    }

    /// <summary>
    /// Shuts down the application. Handles both the Editor and Build versions.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game Requested");

#if UNITY_EDITOR
        // This stops the "Play" mode in the Unity Editor
        EditorApplication.isPlaying = false;
#else
            // This closes the actual .exe or app build
            Application.Quit();
#endif
    }
}