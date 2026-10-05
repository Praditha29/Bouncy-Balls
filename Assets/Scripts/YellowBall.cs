using UnityEngine;
using UnityEngine.InputSystem;

public class YellowBall : MonoBehaviour
{
    private CollectibleSpawner _collectableSpawner;

    private Collider2D ballCollider;

    [SerializeField]
    private int spawnAmount = 4;
    AudioManager audioManager;

    private void Awake()
    {
        _collectableSpawner = FindFirstObjectByType<CollectibleSpawner>();

        ballCollider = GetComponent<Collider2D>();

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        //if (_collectableSpawner == null)
        //{
        //    Debug.LogError("CollectibleSpawner NOT FOUND!");
        //}

        //if (ballCollider == null)
        //{
        //    Debug.LogError("Yellow Ball has no Collider2D!");
        //}
    }

    void Update()
    {
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
        {
            CheckClick(Mouse.current.position.ReadValue());
        }

        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
        {
            CheckClick(
                Touchscreen.current.primaryTouch.position.ReadValue()
            );
        }
    }

    void CheckClick(Vector2 position)
    {
        Vector2 worldPosition =
            Camera.main.ScreenToWorldPoint(position);

        if (ballCollider.OverlapPoint(worldPosition))
        {
            Debug.Log("Yellow ball clicked");
            ScoreManager.Instance.AddScore(1);

            //to let the collectable Spawner script know that one ball is popped
            _collectableSpawner.BallPopped();

            _collectableSpawner.SpawnCollectable(spawnAmount);
         

            Destroy(gameObject);
            audioManager.PlaySFX(audioManager.BallPop);
        }
    }
}