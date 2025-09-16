using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Transform playerCamera;   // assign Main Camera
    public Transform cameraAnchor;   // assign Player/CameraAnchor

    [Header("Player Respawn")]
    public Transform playerSpawn;
    public GameObject player; 
    public float respawnDelay = 1.25f;

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
        //Debug.Log("[GM] OnPlayerDied received, starting respawn coroutine");
        StartCoroutine(RespawnPlayer());
    }

    private System.Collections.IEnumerator RespawnPlayer()
    {
        //Debug.Log("[GM] Respawn waiting " + respawnDelay + "s (realtime)");
        yield return new WaitForSecondsRealtime(respawnDelay);  // <-- unscaled
        //Debug.Log("[GM] Respawning now");

        if (!player) yield break;

        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

        if (cc) cc.enabled = true;

        var hp = player.GetComponent<Health>();
        if (hp) hp.Revive();

        // re-enable control & clear death state before showing the player
        var death = player.GetComponent<PlayerDeathHandler>();
        if (death) death.SetDeadState(false);

        player.SetActive(true);

        // Safeguard: explicitly re-enable control scripts
        var controls = player.GetComponents<MonoBehaviour>();
        foreach (var c in controls)
        {
            if (c is PlayerMotor || c is PlayerCombat)
                c.enabled = true;
        }
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
