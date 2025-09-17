using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject hud;           // gameplay HUD panel
    public GameObject pauseMenu;     // pause menu panel

    [Header("Controls")]
    public KeyCode pauseKey = KeyCode.Escape;
    public MonoBehaviour[] disableWhilePaused; // ex: PlayerMotor, camera look, etc.

    bool paused;

    void Start()
    {
        // start unpaused
        SetPaused(false);
        if (pauseMenu) pauseMenu.SetActive(false);
        if (hud) hud.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
            TogglePause();
    }

    public void TogglePause() => SetPaused(!paused);

    public void SetPaused(bool p)
    {
        paused = p;

        // time & cursor
        Time.timeScale = paused ? 0f : 1f;
        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;

        // panels
        if (hud) hud.SetActive(!paused);
        if (pauseMenu) pauseMenu.SetActive(paused);

        // enable/disable gameplay scripts
        foreach (var m in disableWhilePaused)
            if (m) m.enabled = !paused;
    }

    // Hook these to buttons in the PauseMenu panel
    public void OnResumeButton() => SetPaused(false);

    public void OnRestartButton()
    {
        // reloads the currently active scene
        Time.timeScale = 1f; // reset timescale so it doesn't stay paused
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void OnQuitButton()
    {
        // Make sure time scale is reset (in case game was paused)
        Time.timeScale = 1f;

        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
