using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    public GameObject Pause;

    private bool isPaused = false;

    void Start()
    {
        Pause.SetActive(false);
    }

    public void PauseGame()
    {
        if (isPaused)
            return;
        Pause.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Pause.SetActive(false);

        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}