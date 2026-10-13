using UnityEngine;

// Add this to any special ball prefab (BombBall, etc.).
// The first time one spawns, the matching tutorial plays.
public class SpecialBallTutorialTrigger : MonoBehaviour
{
    [SerializeField] private string tutorialId = "Bomb";

    private void Start()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.TryShowSpecial(tutorialId, gameObject);
    }
}