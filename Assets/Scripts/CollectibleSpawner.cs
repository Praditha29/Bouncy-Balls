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

    public ScoreManager scoreManager;

    [SerializeField]
    private int maximumExtraSpawnDelay = 15;

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

    [SerializeField]
    private int minimumBallsForSpecial = 40;

    private int redYellowSpawnUnlockAt = 0;
    private bool redYellowSpawnCooldown = false;

    //For Boundary Warning
    [SerializeField] private BoundaryWarning boundaryWarning;

    [SerializeField] private int warningBallCount = 120;
    [SerializeField] private int gameOverBallCount = 130;

    private bool warningTriggered = false;
    private bool gameOverTriggered = false;


    private void Awake()
    {
        gameOver = FindFirstObjectByType<GameOver>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
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

    public int GetBallsPopped()
    {
        return ballsPopped;
    }


    public void BallPopped()
    {
        if (gameOver != null && gameOver.IsGameOver())
            return;

        ballsPopped++;
    }

    private int GetTotalBallCount()
    {
        int ballLayer = LayerMask.NameToLayer("Ball");

        if (ballLayer == -1)
        {
            Debug.LogError("A layer named Ball was not found!");
            return 0;
        }

        Transform[] allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);

        int count = 0;

        foreach (Transform obj in allObjects)
        {
            if (obj.gameObject.layer == ballLayer)
            {
                count++;
            }
        }

        return count;
    }
    private int GetExtraSpawnDelay()
    {
        if (scoreManager == null)
            return 0;

        int score = scoreManager.GetScore();

        if (score < 1000)
            return 0;

        int cyclePosition = (score - 1000) % 1000;

        float progress;

        if (cyclePosition < 500)
        {
            progress = cyclePosition / 500f;
        }
        else
        {
            progress = (1000 - cyclePosition) / 500f;
        }

        return Mathf.RoundToInt(progress * maximumExtraSpawnDelay);
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
        
        if (spawned < amount && ballsPopped >= nextBlueSpecialBallAt && CountBalls<BlueSpecialBall>() == 0 && 
            CountBalls<BlueBall>() >= minimumBallsForSpecial)
        {
            SpawnObject(blueSpecialBallPrefab);
            spawned++;
            SetNextBlueSpecialBallCount();
        }

        // -----------------------------
        // GOLD SPECIAL BALL
        // -----------------------------

        if (spawned < amount && ballsPopped >= nextGoldSpecialBallAt && CountBalls<GoldSpecialBall>() == 0
            && CountBalls<GoldBall>() >= minimumBallsForSpecial)
        {
            SpawnObject(goldSpecialBallPrefab);
            spawned++;
            SetNextGoldSpecialBallCount();
        }

        // -----------------------------
        // GREEN SPECIAL BALL
        // -----------------------------

        if (spawned < amount && ballsPopped >= nextGreenSpecialBallAt && CountBalls<GreenSpecialBall>() == 0
            && CountBalls<GreenBall>() >= minimumBallsForSpecial)
        {
            SpawnObject(greenSpecialBallPrefab);
            spawned++;
            SetNextGreenSpecialBallCount();
        }


        // -----------------------------
        // PURPLE SPECIAL BALL
        // -----------------------------

        if (spawned < amount && ballsPopped >= nextPurpleSpecialBallAt && CountBalls<PurpleSpecialBall>() == 0
            && CountBalls<PurpleBall>() >= minimumBallsForSpecial)
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
            GameObject prefab = GetAllowedNormalPrefab();

            // No eligible prefab was found
            if (prefab == null)
                break;

            SpawnObject(prefab);
            spawned++;
        }
    }

    private GameObject GetAllowedNormalPrefab()
    {
        bool allowRedYellow = CanSpawnRedYellow();

        // Build a list of eligible prefabs
        System.Collections.Generic.List<GameObject> allowedPrefabs = new System.Collections.Generic.List<GameObject>();

        foreach (GameObject prefab in collectablePrefabs)
        {
            if (prefab == null)
                continue;

            bool isRedOrYellow = prefab.GetComponent<RedBall>() != null || prefab.GetComponent<YellowBall>() != null;

            if (!allowRedYellow && isRedOrYellow)
                continue;

            allowedPrefabs.Add(prefab);
        }

        if (allowedPrefabs.Count == 0)
        {
            Debug.LogWarning("No eligible normal ball prefabs available!");
            return null;
        }

        return allowedPrefabs[Random.Range(0, allowedPrefabs.Count)];
    }

    public void SpecialBallUsed()
    {
        redYellowSpawnCooldown = true;
        redYellowSpawnUnlockAt = ballsPopped + 30;

        Debug.Log(
            "Special ball used! Red/Yellow spawning paused until " + redYellowSpawnUnlockAt + " total pops. Current pops: " + ballsPopped
        );
    }

    private bool CanSpawnRedYellow()
    {
        if (!redYellowSpawnCooldown)
            return true;

        if (ballsPopped >= redYellowSpawnUnlockAt)
        {
            redYellowSpawnCooldown = false;
            return true;
        }

        return false;
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
        nextBombAt = ballsPopped + Random.Range(15, 26);

        Debug.Log("Next bomb after " + nextBombAt + " balls popped");
    }


    private void SetNextSpikeCount()
    {
        nextSpikeAt = ballsPopped + Random.Range(15, 26);

        Debug.Log("Next spike after " + nextSpikeAt + " balls popped");
    }


    private void SetNextRandomBallCount()
    {
        nextRandomBallAt = ballsPopped + Random.Range(15, 21) + GetExtraSpawnDelay(); 

        Debug.Log("Next Random Ball after " + nextRandomBallAt + " balls popped");
    }

    private void SetNextBlueSpecialBallCount()
    {
        nextBlueSpecialBallAt = ballsPopped + Random.Range(20, 25) + GetExtraSpawnDelay();

        Debug.Log("Next Blue Special Ball after " + nextBlueSpecialBallAt + " balls popped");
    }

    private void SetNextGoldSpecialBallCount()
    {
        nextGoldSpecialBallAt = ballsPopped + Random.Range(25,31) + GetExtraSpawnDelay();

        Debug.Log("Next Gold Special Ball after " + nextGoldSpecialBallAt + " balls popped");
    }
    private void SetNextGreenSpecialBallCount()
    {
        nextGreenSpecialBallAt = ballsPopped + Random.Range(20,31) + GetExtraSpawnDelay();

        Debug.Log("Next Green Special Ball after " + nextGreenSpecialBallAt + " balls popped");
    }

    private void SetNextPurpleSpecialBallCount()
    {
        nextPurpleSpecialBallAt = ballsPopped + Random.Range(20, 31) + GetExtraSpawnDelay();

        Debug.Log("Next Purple Special Ball after " + nextPurpleSpecialBallAt + " balls popped");
    }

    private int CountBalls<T>() where T : MonoBehaviour
    {
        T[] balls = FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        return balls.Length;
    }



    private void Update()
    {
        if (gameOver != null && gameOver.IsGameOver())
            return;

        int totalBalls = GetTotalBallCount();

        // Flash whenever the count crosses above 120 again
        if (totalBalls > warningBallCount && !warningTriggered)
        {
            warningTriggered = true;

            if (boundaryWarning != null)
            {
                boundaryWarning.FlashRedTwice();
            }

            Debug.Log("Boundary warning triggered at " + totalBalls + " balls.");
        }

        // Allow another warning after the count drops to 120 or below
        if (totalBalls <= warningBallCount)
        {
            warningTriggered = false;
        }

        // Game over at 130 balls
        if (!gameOverTriggered && totalBalls >= gameOverBallCount)
        {
            gameOverTriggered = true;

            if (gameOver != null)
            {
                gameOver.Lost();
            }
        }
    }


    public void PrepareForRevive()
    {
        int targetCount = Mathf.Min(
            warningBallCount,
            gameOverBallCount - 1
        );

        int ballLayer = LayerMask.NameToLayer("Ball");

        if (ballLayer == -1)
        {
            Debug.LogError("Layer named Ball was not found.");
            return;
        }

        Transform[] allObjects =
            FindObjectsByType<Transform>(
                FindObjectsSortMode.None
            );

        System.Collections.Generic.List<GameObject> balls =
            new System.Collections.Generic.List<GameObject>();

        foreach (Transform obj in allObjects)
        {
            if (obj.gameObject.layer == ballLayer)
            {
                balls.Add(obj.gameObject);
            }
        }

        // Remove excess balls so the game can resume safely.
        while (balls.Count > targetCount)
        {
            int randomIndex = Random.Range(0, balls.Count);

            GameObject ball = balls[randomIndex];
            balls.RemoveAt(randomIndex);

            if (ball != null)
                Destroy(ball);
        }

        warningTriggered = false;
        gameOverTriggered = false;

        Debug.Log(
            "Revive recovery complete. Excess balls cleared."
        );
    }



}