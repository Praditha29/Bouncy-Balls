using UnityEngine;
using UnityEngine.InputSystem;

public class BlueSpecialBall : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D playableArea;

    private Collider2D specialBallCollider;
    private GameOver gameOver;
    private CollectibleSpawner _collectableSpawner;

    private void Awake()
    {
        _collectableSpawner = FindFirstObjectByType<CollectibleSpawner>();
        specialBallCollider = GetComponent<Collider2D>();
        gameOver = FindFirstObjectByType<GameOver>();
    }

    private void Update()
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
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue()
            );
        }
    }

    private void CheckClick(Vector2 screenPosition)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        if (specialBallCollider.OverlapPoint(worldPosition))
        {
            RemoveAllBlueBalls();

            Destroy(gameObject);
        }
    }

    private void RemoveAllBlueBalls()
    {
        BlueBall[] blueBalls = FindObjectsByType<BlueBall>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        Debug.Log("Blue balls found: " + blueBalls.Length);

        int ballsRemoved = blueBalls.Length;

        foreach (BlueBall blueBall in blueBalls)
            {
                Debug.Log("Removing: " + blueBall.gameObject.name);

                if (_collectableSpawner != null)
                {
                    _collectableSpawner.SpecialBallUsed();
                }
            Destroy(blueBall.gameObject);
            }
        

        // Award 10 points for every 10 balls removed
        int pointsEarned = (ballsRemoved / 10) * 10;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointsEarned);
        }
    }
}