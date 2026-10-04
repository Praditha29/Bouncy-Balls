using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] collectablePrefabs;

    public void SpawnCollectable(Vector2 position)
    {
        int index = Random.Range(0, collectablePrefabs.Length);

        GameObject selectedCollectable =
            collectablePrefabs[index];

        Instantiate(
            selectedCollectable,
            position,
            Quaternion.identity
        );
    }
}