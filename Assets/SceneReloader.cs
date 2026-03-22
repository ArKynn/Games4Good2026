using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    /// <summary>
    /// Reloads the currently active scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        // Get the index of the scene that is currently open
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Load it again
        SceneManager.LoadScene(currentSceneIndex);
    }

    /// <summary>
    /// Loads a specific scene by its name (set in Build Settings).
    /// </summary>
    /// <param name="sceneName">The exact name of the scene</param>
    public void LoadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}