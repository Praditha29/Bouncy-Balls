using UnityEngine;

public class DestroyFloatingText : MonoBehaviour
{
    [SerializeField] private float destroyTime = 1.5f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
