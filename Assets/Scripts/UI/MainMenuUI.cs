using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene to Load")]
    [Tooltip("Name of your gameplay scene (the one you've been working in). Make sure it's in Build Settings!")]
    public string gameSceneName = "SampleScene";

    [Header("Cursor")]
    public bool showCursor = true;

    void OnEnable()
    {
        // Ensure the game isn't paused if returning here
        Time.timeScale = 1f;

        // Friendly cursor defaults for menus
        if (showCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // Hook this to the Start button
    public void OnStartGame()
    {
        // Optional: add a small SFX/animation here
        SceneManager.LoadScene(gameSceneName);
    }

    // Hook this to a Quit button
    public void OnQuit()
    {
#if UNITY_EDITOR
        // Works in Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Works in builds
        Application.Quit();
#endif
    }
}
