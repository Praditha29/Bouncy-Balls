using UnityEngine;
using UnityEngine.InputSystem;

public class RandomBall : MonoBehaviour
{
    [SerializeField]
    private GameObject[] ballPrefabs;

    [SerializeField]
    private BoxCollider2D playableArea;

    private Collider2D randomBallCollider;
    private GameOver gameOver;

    private void Awake()
    {
        randomBallCollider = GetComponent<Collider2D>();
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
            CheckClick(
                Touchscreen.current.primaryTouch.position.ReadValue()
            );
        }
    }

    private void CheckClick(Vector2 screenPosition)
    {
        Vector2 worldPosition =
            Camera.main.ScreenToWorldPoint(screenPosition);

        if (randomBallCollider.OverlapPoint(worldPosition))
        {
            RandomizeBalls();

            // Remove the Random Ball itself
            Destroy(gameObject);
        }
    }

    private void RandomizeBalls()
    {
        // 50% chance of adding or removing
        bool addBalls = Random.value > 0.5f;

        if (addBalls)
        {
            AddBalls(10);
        }
        else
        {
            RemoveBalls(10);
        }
    }

    private void AddBalls(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int randomIndex =
                Random.Range(0, ballPrefabs.Length);

            GameObject selectedBall =
                ballPrefabs[randomIndex];

            float randomX = Random.Range(
                playableArea.bounds.min.x,
                playableArea.bounds.max.x
            );

            float randomY = Random.Range(
                playableArea.bounds.min.y,
                playableArea.bounds.max.y
            );

            Vector2 spawnPosition =
                new Vector2(randomX, randomY);

            Instantiate(
                selectedBall,
                spawnPosition,
                Quaternion.identity
            );
        }
    }

    private void RemoveBalls(int amount)
    {
        GameObject[] balls =
            GameObject.FindGameObjectsWithTag("Ball");

        // Shuffle the array
        for (int i = 0; i < balls.Length; i++)
        {
            int randomIndex =
                Random.Range(i, balls.Length);

            GameObject temp = balls[i];
            balls[i] = balls[randomIndex];
            balls[randomIndex] = temp;
        }

        // Remove up to 10 balls
        int removeCount =
            Mathf.Min(amount, balls.Length);

        for (int i = 0; i < removeCount; i++)
        {
            Destroy(balls[i]);
        }
    }
}