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

    private bool gameOver = false;

    [SerializeField]
    private TMPro.TextMeshProUGUI finalScoreText;

    AudioManager audioManager;

    //[SerializeField]
    //private TMPro.TextMeshProUGUI highScoreText;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }



    private void Start()
    {
        Time.timeScale = 1f;

        // Hide Game Over Canvas at the start
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
        // Find all objects inside the playable area
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


    // PUBLIC so CollectibleSpawner can call this
    public void Lost()
    {
        if (gameOver)
            return;

        gameOver = true;

        Debug.Log("GAME OVER!");
        audioManager.PlaySFX(audioManager.GameOverAudio);


        // Show entire Game Over Canvas
        gameOverCanvas.SetActive(true);


        finalScoreText.text =
            "Score: " +
            ScoreManager.Instance.GetScore();


        //highScoreText.text =
        //    "High Score: " +
        //    ScoreManager.Instance.GetHighScore();


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