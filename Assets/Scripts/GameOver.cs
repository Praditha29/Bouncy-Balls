using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField]
    private GameObject youLostText;

    [SerializeField]
    private TMPro.TextMeshProUGUI finalScoreText;

    [SerializeField]
    private TMPro.TextMeshProUGUI highScoreText;

    private bool gameOver = false;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        youLostText.SetActive(false);
    }

    private void Update()
    {
        if (gameOver)
            return;

        CheckNoBalls();
    }

    void CheckNoBalls()
    {
        Collider2D[] allColliders =
            FindObjectsByType<Collider2D>(
                FindObjectsSortMode.None
            );

        int ballCount = 0;

        foreach (Collider2D col in allColliders)
        {
            if (col.gameObject.layer ==
                LayerMask.NameToLayer("Ball"))
            {
                ballCount++;
            }
        }

        if (ballCount == 0)
        {
            Lost();
        }
    }

    public void Lost()
    {
        if (gameOver)
            return;

        gameOver = true;

        Debug.Log("GAME OVER!");

        youLostText.SetActive(true);

        finalScoreText.text =
            "Score: " +
            ScoreManager.Instance.GetScore();

        highScoreText.text =
            "High Score: " +
            ScoreManager.Instance.GetHighScore();

        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}