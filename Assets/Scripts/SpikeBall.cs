using UnityEngine;
using UnityEngine.InputSystem;

public class SpikeBall : MonoBehaviour
{
    private CollectibleSpawner _collectableSpawner;
    private Collider2D spikeCollider;

    AudioManager audioManager;

    private GameOver gameOver;

    private void Awake()
    {
        _collectableSpawner = FindFirstObjectByType<CollectibleSpawner>();
        spikeCollider = GetComponent<Collider2D>();
        gameOver = FindFirstObjectByType<GameOver>();

        audioManager = GameObject.FindGameObjectWithTag("Audio")
            .GetComponent<AudioManager>();
    }

    void Update()
    {
        if (gameOver.IsGameOver())
            return;
        // Don't allow popping while paused
        if (Time.timeScale == 0f)
            return;

        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            CheckClick(Mouse.current.position.ReadValue());

        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue());
    }

    void CheckClick(Vector2 position)
    {
        Vector2 worldPosition =
            Camera.main.ScreenToWorldPoint(position);

        if (spikeCollider.OverlapPoint(worldPosition))
        {
            Debug.Log("Spike Ball Clicked");

            // End the game
            gameOver.Lost();

            // Play bomb sound
            audioManager.PlaySFX(audioManager.Bomb);

            // Destroy Spike Ball
            Destroy(gameObject);
        }
    }
}