using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] collectablePrefabs;

    [SerializeField]
    private BoxCollider2D boundary;

    public void SpawnCollectable()
    {
        // Pick a random ball
        int index = Random.Range(0, collectablePrefabs.Length);

        GameObject selectedCollectable =
            collectablePrefabs[index];

        // Pick a random position inside the boundary
        float randomX = Random.Range(
            boundary.bounds.min.x,
            boundary.bounds.max.x
        );

        float randomY = Random.Range(
            boundary.bounds.min.y,
            boundary.bounds.max.y
        );

        Vector2 randomPosition = new Vector2(randomX, randomY);

        // Spawn the ball
        Instantiate(
            selectedCollectable,
            randomPosition,
            Quaternion.identity
        );
    }
}