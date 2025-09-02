using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void Play() { SceneManager.LoadScene("Level01"); }
    public void Quit() { Application.Quit(); }
}
