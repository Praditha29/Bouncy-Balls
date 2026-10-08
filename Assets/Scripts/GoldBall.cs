using UnityEngine;
using UnityEngine.InputSystem;

public class GoldBall : MonoBehaviour
{
    private CollectibleSpawner _collectableSpawner;

    private Collider2D ballCollider;

    [SerializeField]
    private int spawnAmount = 5;

    private GameOver gameOver;

    //for floating Text
    [SerializeField] private GameObject floatingTextPrefab;

    AudioManager audioManager;

    private void Awake()
    {
        _collectableSpawner = FindFirstObjectByType<CollectibleSpawner>();
        ballCollider = GetComponent<Collider2D>();
        gameOver = FindFirstObjectByType<GameOver>();

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        //if (_collectableSpawner == null)
        //{
        //    Debug.LogError("CollectibleSpawner NOT FOUND!");
        //}
        //else
        //{
        //    Debug.Log("CollectibleSpawner FOUND!");
        //}
    }

    void Update()
    {
        if (gameOver.IsGameOver())
            return;
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            CheckClick(Mouse.current.position.ReadValue());

        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue());
    }

    void CheckClick(Vector2 position)
    {
        // Convert screen position to world position
        Vector2 worldPosition =
            Camera.main.ScreenToWorldPoint(position);

        // Check if this ball was clicked
        if (ballCollider.OverlapPoint(worldPosition))
        {
            Debug.Log("Gold ball clicked");
            ScoreManager.Instance.AddScore(1);

            //to let the collectable Spawner script know that one ball is popped
            _collectableSpawner.BallPopped();

            // Spawn 4 random balls
            _collectableSpawner.SpawnCollectable(spawnAmount);

            //for Floating Text
            Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);

            // Destroy gold  ball
            Destroy(gameObject);
            audioManager.PlaySFX(audioManager.BallPop);
        }
    }
}