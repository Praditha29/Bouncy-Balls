using UnityEngine;

public class HowToPlay : MonoBehaviour
{
    [SerializeField]
    private GameObject howToPlayPanel;

    public void OpenHowToPlay()
    {
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);
    }
}   