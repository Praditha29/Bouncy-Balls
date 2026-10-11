
using System.Collections;
using UnityEngine;

public class BoundaryWarning : MonoBehaviour
{
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float flashDuration = 0.25f;

    private SpriteRenderer[] boundaryRenderers;
    private Color[] originalColors;
    

    private void Awake()
    {
        boundaryRenderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[boundaryRenderers.Length];

        for (int i = 0; i < boundaryRenderers.Length; i++)
        {
            originalColors[i] = boundaryRenderers[i].color;
        }
    }

    public void FlashRedTwice()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int flash = 0; flash < 2; flash++)
        {
            SetBoundaryColor(warningColor);
            yield return new WaitForSecondsRealtime(flashDuration);

            RestoreOriginalColors();
            yield return new WaitForSecondsRealtime(flashDuration);
        }

        RestoreOriginalColors();
    }

    private void SetBoundaryColor(Color color)
    {
        foreach (SpriteRenderer renderer in boundaryRenderers)
        {
            renderer.color = color;
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < boundaryRenderers.Length; i++)
        {
            boundaryRenderers[i].color = originalColors[i];
        }
    }
}
