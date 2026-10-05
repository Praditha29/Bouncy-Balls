using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D playableArea;

    [SerializeField]
    private GameObject gameOverCanvas;

    [SerializeField]
    private int maxBalls = 20;

    [SerializeField]
    private TMPro.TextMeshProUGUI finalScoreText;

    private bool gameOver = false;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio")
            .GetComponent<AudioManager>();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        gameOverCanvas.SetActive(false);
    }

    private void Update()
    {
        if (gameOver)
            return;

        CheckGameOver();
    }

    void CheckGameOver()
    {
        Collider2D[] objectsInside =
            Physics2D.OverlapBoxAll(
                playableArea.bounds.center,
                playableArea.bounds.size,
                0f
            );

        int ballCount = 0;

        foreach (Collider2D obj in objectsInside)
        {
            if (obj.gameObject.layer ==
                LayerMask.NameToLayer("Ball"))
            {
                ballCount++;
            }
        }

        // No balls left
        if (ballCount == 0)
        {
            Lost();
        }

        // Too many balls
        else if (ballCount >= maxBalls)
        {
            Lost();
        }
    }

    public void Lost()
    {
        if (gameOver)
            return;

        // Mark game as over FIRST
        gameOver = true;

        Debug.Log("GAME OVER!");

        // Get the final score BEFORE stopping the game
        int finalScore = ScoreManager.Instance.GetScore();

        // Display final score
        finalScoreText.text = "Score: " + finalScore;

        // Show Game Over UI
        gameOverCanvas.SetActive(true);

        // Play sound
        audioManager.PlaySFX(audioManager.GameOverAudio);

        // Stop game
        Time.timeScale = 0f;
    }

    public bool IsGameOver()
    {
        return gameOver;
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