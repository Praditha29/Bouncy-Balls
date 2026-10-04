using UnityEngine;
using UnityEngine.InputSystem;

public class BombBall : MonoBehaviour
{
    public float blastRadius = 3f;
    [SerializeField]
    private LayerMask ballLayer;


    private void Update()
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

        // Check which collider was clicked
        Collider2D clickedCollider =
            Physics2D.OverlapPoint(worldPosition);

        // Check if the clicked collider belongs to this bomb
        if (clickedCollider == GetComponent<Collider2D>())
        {
            Debug.Log("Bomb clicked!");

            BlastBalls();

            Destroy(gameObject);
        }
    }

    void BlastBalls()
    {
        Collider2D[] balls =
            Physics2D.OverlapCircleAll(
                transform.position,
                blastRadius,
                ballLayer
            );

        foreach (Collider2D ball in balls)
        {
            // Only destroy objects on the Ball layer
            if (ball.gameObject.layer == LayerMask.NameToLayer("Ball"))
            {
                if (ball.gameObject != gameObject)
                {
                    Destroy(ball.gameObject);
                }
            }
        }
    }

    // Shows the blast radius in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}