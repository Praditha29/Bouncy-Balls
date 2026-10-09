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

    [SerializeField]
    private GameObject blueSpecialBallPrefab;
    private int nextBlueSpecialBallAt;

    [SerializeField]
    private GameObject goldSpecialBallPrefab;
    private int nextGoldSpecialBallAt;

    [SerializeField]
    private GameObject greenSpecialBallPrefab;
    private int nextGreenSpecialBallAt;

    [SerializeField]
    private GameObject purpleSpecialBallPrefab;
    private int nextPurpleSpecialBallAt;

    private void Awake()
    {
        gameOver = FindFirstObjectByType<GameOver>();
    }


    private void Start()
    {
        SetNextBombCount();
        SetNextSpikeCount();
        SetNextRandomBallCount();
        SetNextBlueSpecialBallCount();
        SetNextGoldSpecialBallCount();
        SetNextGreenSpecialBallCount();
        SetNextPurpleSpecialBallCount();
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
        // BLUE SPECIAL BALL
        // -----------------------------

        if (ballsPopped >= nextBlueSpecialBallAt && spawned < amount)
        {
            SpawnObject(blueSpecialBallPrefab);

            spawned++;

            SetNextBlueSpecialBallCount();
        }

        // -----------------------------
        // GOLD SPECIAL BALL
        // -----------------------------

        if (ballsPopped >= nextGoldSpecialBallAt && spawned < amount)
        {
            SpawnObject(goldSpecialBallPrefab);

            spawned++;

            SetNextGoldSpecialBallCount();
        }

        // -----------------------------
        // GREEN SPECIAL BALL
        // -----------------------------

        if (ballsPopped >= nextGreenSpecialBallAt && spawned < amount)
        {
            SpawnObject(greenSpecialBallPrefab);

            spawned++;

            SetNextGreenSpecialBallCount();
        }

        // -----------------------------
        // PURPLE SPECIAL BALL
        // -----------------------------

        if (ballsPopped >= nextPurpleSpecialBallAt && spawned < amount)
        {
            SpawnObject(purpleSpecialBallPrefab);

            spawned++;

            SetNextPurpleSpecialBallCount();
        }

        // -----------------------------
        // NORMAL BALLS
        // -----------------------------

        while (spawned < amount)
        {
            int index = Random.Range(0, collectablePrefabs.Length);

            SpawnObject(collectablePrefabs[index]);

            spawned++;
        }
    }


    private void SpawnObject(GameObject prefab)
    {
        float randomX = Random.Range(boundary.bounds.min.x, boundary.bounds.max.x);

        float randomY = Random.Range(boundary.bounds.min.y, boundary.bounds.max.y);

        Vector2 spawnPosition = new Vector2(randomX, randomY);

        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }


    private int GetBombCount()
    {
        BombBall[] bombs = FindObjectsByType<BombBall>(FindObjectsSortMode.None);

        return bombs.Length;
    }


    private void SetNextBombCount()
    {
        nextBombAt = ballsPopped + Random.Range(6, 11);

        Debug.Log("Next bomb after " + nextBombAt + " balls popped");
    }


    private void SetNextSpikeCount()
    {
        nextSpikeAt = ballsPopped + Random.Range(10, 16);

        Debug.Log("Next spike after " + nextSpikeAt + " balls popped");
    }


    private void SetNextRandomBallCount()
    {
        nextRandomBallAt = ballsPopped + Random.Range(15, 21);

        Debug.Log("Next Random Ball after " + nextRandomBallAt + " balls popped");
    }

    private void SetNextBlueSpecialBallCount()
    {
        nextBlueSpecialBallAt = ballsPopped + Random.Range(10, 16);

        Debug.Log("Next Blue Special Ball after " + nextBlueSpecialBallAt + " balls popped");
    }

    private void SetNextGoldSpecialBallCount()
    {
        nextGoldSpecialBallAt = ballsPopped + Random.Range(10, 16);

        Debug.Log("Next Gold Special Ball after " + nextGoldSpecialBallAt + " balls popped");
    }
    private void SetNextGreenSpecialBallCount()
    {
        nextGreenSpecialBallAt = ballsPopped + Random.Range(10, 16);

        Debug.Log("Next Green Special Ball after " + nextGreenSpecialBallAt + " balls popped");
    }

    private void SetNextPurpleSpecialBallCount()
    {
        nextPurpleSpecialBallAt = ballsPopped + Random.Range(10, 16);

        Debug.Log("Next Purple Special Ball after " + nextPurpleSpecialBallAt + " balls popped");
    }
}