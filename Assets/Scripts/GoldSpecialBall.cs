using UnityEngine;
using UnityEngine.InputSystem;

public class GoldSpecialBall : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D playableArea;

    private Collider2D specialBallCollider;
    private GameOver gameOver;


    private void Awake()
    {
        specialBallCollider = GetComponent<Collider2D>();
        gameOver = FindFirstObjectByType<GameOver>();
    }
    
   

    // Update is called once per frame
    void Update()
    {
        if (gameOver != null && gameOver.IsGameOver())
            return;
        // Don't allow popping while paused
        if (Time.timeScale == 0f)
            return;

        // Mouse click
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
        {
            CheckClick(Mouse.current.position.ReadValue());
        }

        // Touch
        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
        {
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void CheckClick(Vector2 screenPosition)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        if (specialBallCollider.OverlapPoint(worldPosition))
        {
            RemoveAllGoldBalls();

            Destroy(gameObject);
        }
    }

    private void RemoveAllGoldBalls()
    {
        GoldBall[] goldBalls = FindObjectsByType<GoldBall>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        Debug.Log("Blue balls found: " + goldBalls.Length);

        foreach (GoldBall goldBall in goldBalls)
        {
            Debug.Log("Removing: " + goldBall.gameObject.name);

            Destroy(goldBall.gameObject);
        }

        int ballsRemoved = goldBalls.Length;

        // Award 10 points for every 10 balls removed
        int pointsEarned = (ballsRemoved / 10) * 10;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointsEarned);
        }
    }
}

