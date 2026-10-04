using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] collectablePrefabs;

    [SerializeField]
    private GameObject bombPrefab;

    [SerializeField]
    private BoxCollider2D boundary;

    [SerializeField]
    private float spawnRadius = 0.5f;

    private int ballsPopped = 0;
    private int nextBombAt;


    int GetBombCount()
    {
        BombBall[] bombs =
            FindObjectsByType<BombBall>(
                FindObjectsSortMode.None
            );

        return bombs.Length;
    }

    private void Start()
    {
        SetNextBombCount();
    }

    public void BallPopped()
    {
        ballsPopped++;
    }

    public void SpawnCollectable()
    {
        GameObject selectedCollectable;

        if (ballsPopped >= nextBombAt && GetBombCount() < 3)
        {
            selectedCollectable = bombPrefab;
            SetNextBombCount();
        }
        else
        {
            int index = Random.Range(
                0,
                collectablePrefabs.Length
            );

            selectedCollectable = collectablePrefabs[index];
        }

        if (TryGetValidSpawnPosition(out Vector2 spawnPosition))
        {
            Instantiate(
                selectedCollectable,
                spawnPosition,
                Quaternion.identity
            );
        }
        else
        {
            Debug.Log("PLAYABLE AREA IS FULL!");

            GameOver gameOver =
                FindFirstObjectByType<GameOver>();

            if (gameOver != null)
            {
                gameOver.Lost();
            }
        }
    }

    bool TryGetValidSpawnPosition(out Vector2 spawnPosition)
    {
        for (int i = 0; i < 50; i++)
        {
            float randomX = Random.Range(
                boundary.bounds.min.x + spawnRadius,
                boundary.bounds.max.x - spawnRadius
            );

            float randomY = Random.Range(
                boundary.bounds.min.y + spawnRadius,
                boundary.bounds.max.y - spawnRadius
            );

            Vector2 randomPosition =
                new Vector2(randomX, randomY);

            Collider2D[] objects =
                Physics2D.OverlapCircleAll(
                    randomPosition,
                    spawnRadius
                );

            bool positionFree = true;

            foreach (Collider2D obj in objects)
            {
                if (obj.gameObject.layer ==
                    LayerMask.NameToLayer("Ball"))
                {
                    positionFree = false;
                    break;
                }
            }

            if (positionFree)
            {
                spawnPosition = randomPosition;
                return true;
            }
        }

        spawnPosition = Vector2.zero;
        return false;
    }

    void SetNextBombCount()
    {
        nextBombAt =
            ballsPopped + Random.Range(8, 11);

        Debug.Log(
            "Next bomb after " +
            nextBombAt +
            " balls popped"
        );
    }
}