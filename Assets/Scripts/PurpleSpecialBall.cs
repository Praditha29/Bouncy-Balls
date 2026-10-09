using UnityEngine;
using UnityEngine.InputSystem;

public class PurpleSpecialBall : MonoBehaviour
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
            RemoveAllPurpleBalls();

            Destroy(gameObject);
        }
    }

    private void RemoveAllPurpleBalls()
    {
        PurpleBall[] purpleBalls = FindObjectsByType<PurpleBall>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        Debug.Log("Purple balls found: " + purpleBalls.Length);

        foreach (PurpleBall purpleBall in purpleBalls)
        {
            Debug.Log("Removing: " + purpleBall.gameObject.name);

            Destroy(purpleBall.gameObject);
        }

        int ballsRemoved = purpleBalls.Length;
        // Award 10 points for every 10 balls removed
        int pointsEarned = (ballsRemoved / 10) * 10;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointsEarned);
        }
    }
}

