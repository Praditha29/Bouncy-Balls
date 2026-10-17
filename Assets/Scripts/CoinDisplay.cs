using UnityEngine;
using TMPro;

public class CoinDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private string prefix = "";

    private void Start()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinsChanged += UpdateText;
            UpdateText(CoinManager.Instance.TotalCoins);
        }
        else
        {
            UpdateText(0);
        }
    }

    private void OnDestroy()
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.OnCoinsChanged -= UpdateText;
    }

    private void UpdateText(int total)
    {
        if (coinText != null)
            coinText.text = prefix + total;
    }
}