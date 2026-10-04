using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] collectablePrefabs;

    [SerializeField]
    private GameObject bombPrefab;

    [SerializeField]
    private BoxCollider2D boundary;


    private int ballsPopped = 0;
    private int nextBombAt;


    private void Start()
    {
        SetNextBombCount();
    }


    public void BallPopped()
    {
        ballsPopped++;
    }


    public void SpawnCollectable(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject selectedCollectable;


            // Check if it is time to spawn a bomb
            if (ballsPopped >= nextBombAt &&
                GetBombCount() < 4)
            {
                selectedCollectable = bombPrefab;

                SetNextBombCount();
            }
            else
            {
                int index =
                    Random.Range(
                        0,
                        collectablePrefabs.Length
                    );

                selectedCollectable =
                    collectablePrefabs[index];
            }


            // Random position inside the boundary
            float randomX =
                Random.Range(
                    boundary.bounds.min.x,
                    boundary.bounds.max.x
                );

            float randomY =
                Random.Range(
                    boundary.bounds.min.y,
                    boundary.bounds.max.y
                );


            Vector2 spawnPosition =
                new Vector2(
                    randomX,
                    randomY
                );


            Instantiate(
                selectedCollectable,
                spawnPosition,
                Quaternion.identity
            );
        }
    }


    int GetBombCount()
    {
        BombBall[] bombs =
            FindObjectsByType<BombBall>(
                FindObjectsSortMode.None
            );

        return bombs.Length;
    }


    void SetNextBombCount()
    {
        nextBombAt =
            ballsPopped +
            Random.Range(8, 11);


        Debug.Log(
            "Next bomb after " +
            nextBombAt +
            " balls popped"
        );
    }
}