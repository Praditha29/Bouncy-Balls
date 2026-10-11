using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI highScoreText;

    private int score;
    private int highScore;

    private void Awake()
    {
        Instance = this;

        Time.timeScale = 1f;

        highScore = PlayerPrefs.GetInt("HighScore", 0);

        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount;

        // Don't allow score to go below 0
        if (score < 0)
        {
            score = 0;
        }

        if (score > highScore)
        {
            highScore = score;

            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateScoreUI();
    }

    public int GetScore()
    {
        return score;
    }

    public int GetHighScore()
    {
        return highScore;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        if (highScoreText != null)
        {
            highScoreText.text = highScore.ToString();
        }
    }
}