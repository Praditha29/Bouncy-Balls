
using System.Collections;
using UnityEngine;

public class BombAnimationSequence : MonoBehaviour
{
    [SerializeField] private GameObject bombVisual;
    [SerializeField] private GameObject explosionVisual;
    [SerializeField] private float bombAnimationTime = 0.5f;

    private bool hasExploded = false;

    private void Start()
    {
        bombVisual.SetActive(true);
        explosionVisual.SetActive(false);
    }

    public void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;StartCoroutine(PlayExplosion());
    }

    private IEnumerator PlayExplosion()
    {
        // Let the bomb grow first
        yield return new WaitForSeconds(bombAnimationTime);

        // Hide the bomb and show the explosion
        bombVisual.SetActive(false);
        explosionVisual.SetActive(true);

        // Wait for the explosion animation to finish
        Animator explosionAnimator = explosionVisual.GetComponent<Animator>();

        if (explosionAnimator != null)
        {
            yield return null;

            yield return new WaitForSeconds(explosionAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        Destroy(gameObject);
    }
}
