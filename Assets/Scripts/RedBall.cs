using UnityEngine;
using UnityEngine.InputSystem;

public class RedBall : MonoBehaviour
{
    private CollectibleSpawner _collectableSpawner;

    private void Awake()
    {
        _collectableSpawner = FindFirstObjectByType<CollectibleSpawner>();

        if (_collectableSpawner == null)
        {
            Debug.LogError("CollectibleSpawner NOT FOUND!");
        }
        else
        {
            Debug.Log("CollectibleSpawner FOUND!");
        }
    }

    void Update()
    {
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
        if (Physics2D.OverlapPoint(worldPosition) == GetComponent<Collider2D>())
        {
            Debug.Log("Red ball clicked");

            // Spawn random ball
            _collectableSpawner.SpawnCollectable();

            // Destroy red ball
            Destroy(gameObject);
        }
    }
}