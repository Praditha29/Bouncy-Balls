using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI highScoreText;

    private void Start()
    {
        Time.timeScale = 1f;

        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        highScoreText.text = "High Score: " + highScore;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}