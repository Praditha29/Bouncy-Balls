using UnityEngine;
using UnityEngine.InputSystem;

public class BombBall : MonoBehaviour
{
    public float blastRadius = 3f;

    [SerializeField]
    private LayerMask ballLayer;

    private Collider2D bombCollider;
    private bool exploded = false;
    AudioManager audioManager;

    private void Awake()
    {
        bombCollider = GetComponent<Collider2D>();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void Update()
    {
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            CheckClick(Mouse.current.position.ReadValue());

        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
            CheckClick(Touchscreen.current.primaryTouch.position.ReadValue());
    }

    void CheckClick(Vector2 position)
    {
        Vector2 worldPosition =
            Camera.main.ScreenToWorldPoint(position);

        if (bombCollider.OverlapPoint(worldPosition))
        {
            Debug.Log("Bomb clicked!");

            Explode();
            audioManager.PlaySFX(audioManager.Bomb);
        }
    }

    public void Explode()
    {
        // Prevent the same bomb from exploding twice
        if (exploded)
            return;

        exploded = true;

        Collider2D[] balls =
            Physics2D.OverlapCircleAll(
                transform.position,
                blastRadius,
                ballLayer
            );

        int ballsDestroyed = 0;

        foreach (Collider2D ball in balls)
        {
            if (ball.gameObject == gameObject)
                continue;

            BombBall otherBomb =
                ball.GetComponent<BombBall>();

            if (otherBomb != null)
            {
                // Chain reaction!
                otherBomb.Explode();
            }
            else
            {
                // Normal ball
                Destroy(ball.gameObject);
            }

            ballsDestroyed++;
        }

        // Bombs reduce the score
        ScoreManager.Instance.AddScore(-ballsDestroyed);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            blastRadius
        );
    }
}