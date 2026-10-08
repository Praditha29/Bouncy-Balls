using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] collectablePrefabs;

    [SerializeField]
    private GameObject bombPrefab;

    [SerializeField]
    private GameObject spikePrefab;

    [SerializeField]
    private GameObject randomBallPrefab;

    [SerializeField]
    private BoxCollider2D boundary;

    private GameOver gameOver;

    private int ballsPopped = 0;

    private int nextBombAt;
    private int nextSpikeAt;
    private int nextRandomBallAt;


    private void Awake()
    {
        gameOver = FindFirstObjectByType<GameOver>();
    }


    private void Start()
    {
        SetNextBombCount();
        SetNextSpikeCount();
        SetNextRandomBallCount();
    }


    public void BallPopped()
    {
        if (gameOver != null && gameOver.IsGameOver())
            return;

        ballsPopped++;
    }


    public void SpawnCollectable(int amount)
    {
        if (gameOver != null && gameOver.IsGameOver())
            return;

        int spawned = 0;


        // -----------------------------
        // BOMB
        // -----------------------------

        if (ballsPopped >= nextBombAt &&
            GetBombCount() < 4 &&
            spawned < amount)
        {
            SpawnObject(bombPrefab);

            spawned++;

            SetNextBombCount();
        }


        // -----------------------------
        // SPIKE BALL
        // -----------------------------

        if (ballsPopped >= nextSpikeAt &&
            spawned < amount)
        {
            SpawnObject(spikePrefab);

            spawned++;

            SetNextSpikeCount();
        }


        // -----------------------------
        // RANDOM BALL
        // -----------------------------

        if (ballsPopped >= nextRandomBallAt &&
            spawned < amount)
        {
            SpawnObject(randomBallPrefab);

            spawned++;

            SetNextRandomBallCount();
        }


        // -----------------------------
        // NORMAL BALLS
        // -----------------------------

        while (spawned < amount)
        {
            int index =
                Random.Range(0, collectablePrefabs.Length);

            SpawnObject(collectablePrefabs[index]);

            spawned++;
        }
    }


    private void SpawnObject(GameObject prefab)
    {
        float randomX = Random.Range(
            boundary.bounds.min.x,
            boundary.bounds.max.x
        );

        float randomY = Random.Range(
            boundary.bounds.min.y,
            boundary.bounds.max.y
        );

        Vector2 spawnPosition =
            new Vector2(randomX, randomY);

        Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );
    }


    private int GetBombCount()
    {
        BombBall[] bombs =
            FindObjectsByType<BombBall>(
                FindObjectsSortMode.None
            );

        return bombs.Length;
    }


    private void SetNextBombCount()
    {
        nextBombAt =
            ballsPopped + Random.Range(6, 11);

        Debug.Log(
            "Next bomb after " + nextBombAt + " balls popped"
        );
    }


    private void SetNextSpikeCount()
    {
        nextSpikeAt =
            ballsPopped + Random.Range(10, 16);

        Debug.Log(
            "Next spike after " + nextSpikeAt + " balls popped"
        );
    }


    private void SetNextRandomBallCount()
    {
        nextRandomBallAt =
            ballsPopped + Random.Range(15, 21);

        Debug.Log(
            "Next Random Ball after " +
            nextRandomBallAt +
            " balls popped"
        );
    }
}