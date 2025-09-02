using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    [Header("UI")]
    public GameObject hud;
    public GameObject gameOverUI;
    public GameObject winUI;

    [Header("Audio")]
    public AudioSource music;
    public AudioClip winJingle;
    public AudioClip loseJingle;

    public int score;

    void Awake() { if (I == null) I = this; else Destroy(gameObject); }

    public void AddScore(int v) { score += v; }

    public void OnPlayerDied()
    {
        if (hud) hud.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(true);
        if (music && loseJingle) music.PlayOneShot(loseJingle);
        Time.timeScale = 0f;
    }

    public void OnWin()
    {
        if (hud) hud.SetActive(false);
        if (winUI) winUI.SetActive(true);
        if (music && winJingle) music.PlayOneShot(winJingle);
        Time.timeScale = 0f;
    }

    public void Restart() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void Quit() { Application.Quit(); }
}
