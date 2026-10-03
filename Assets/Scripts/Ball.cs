using UnityEngine;
using UnityEngine.InputSystem;

public class Ball : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            CheckClick(Mouse.current.position.ReadValue());

        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue());
    }

    void CheckClick(Vector2 position)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(position);

        if (Physics2D.OverlapPoint(worldPosition) == GetComponent<Collider2D>())
        {
            Debug.Log("clicked");
            Destroy(gameObject);
        }
    }
}