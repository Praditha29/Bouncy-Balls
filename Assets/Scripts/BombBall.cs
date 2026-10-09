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

    
    private GameOver gameOver;

    private Animator bombAnimator;
    private void Awake()
    {
        bombCollider = GetComponent<Collider2D>();
        gameOver = FindFirstObjectByType<GameOver>();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        bombAnimator = GetComponent<Animator>();
    }

    private void Update()
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

        // Bombs Adds the score
        ScoreManager.Instance.AddScore(ballsDestroyed);

        // Play the animation sequence.
        // The animation script will destroy the bomb afterward.

        if (bombAnimator != null)
        {
            bombAnimator.Play("Bomb", 0, 0f);
            StartCoroutine(DestroyAfterAnimation());
        }
        else
        {
            Debug.LogError("Animator component is missing!");
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            blastRadius
        );
    }


    [SerializeField] private float explosionDuration = 0.3f;
    private System.Collections.IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(explosionDuration);
        Destroy(gameObject);
    }
}