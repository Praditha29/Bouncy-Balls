
using UnityEngine;
using TMPro;

public class PowerUpController : MonoBehaviour
{
    [System.Serializable]
    public class PowerUp
    {
        public string powerUpName;
        public GameObject ballPrefab;
        public Transform spawnPoint;
        public TMP_Text countText;
        public int startingCount = 3;

        [HideInInspector]
        public int currentCount;
    }

    [Header("Power-Ups")]
    [SerializeField] private PowerUp[] powerUps;

    [Header("Ad Panel")]
    [SerializeField] private GameObject watchAdPanel;

    private int selectedPowerUpIndex = -1;
    private bool waitingForAdReward;

    private void Start()
    {
        for (int i = 0; i < powerUps.Length; i++)
        {
            powerUps[i].currentCount = powerUps[i].startingCount;

            UpdateCountText(i);
        }

        if (watchAdPanel != null)
            watchAdPanel.SetActive(false);
    }

    public void OnPowerUpClicked(int index)
    {
        if (waitingForAdReward)
            return;

        if (index < 0 || index >= powerUps.Length)
            return;

        PowerUp powerUp = powerUps[index];

        if (powerUp.currentCount > 0)
        {
            powerUp.currentCount--;
            UpdateCountText(index);
            SpawnBall(index);
        }
        else
        {
            selectedPowerUpIndex = index;

            if (watchAdPanel != null)
                watchAdPanel.SetActive(true);
        }
    }

    private void SpawnBall(int index)
    {
        PowerUp powerUp = powerUps[index];

        if (powerUp.ballPrefab == null ||
            powerUp.spawnPoint == null)
        {
            Debug.LogError("Assign prefab and spawn point for " + powerUp.powerUpName);
            return;
        }

        Instantiate(powerUp.ballPrefab, powerUp.spawnPoint.position, Quaternion.identity);
    }

    private void UpdateCountText(int index)
    {
        if (powerUps[index].countText != null)
        {
            powerUps[index].countText.text =
                powerUps[index].currentCount.ToString();
        }
    }

    public void OnWatchAdClicked()
    {
        if (waitingForAdReward ||
            selectedPowerUpIndex < 0)
            return;

        //RewardedAdsManager ads = FindFirstObjectByType<RewardedAdsManager>();

        //if (ads == null)
        //{
            //Debug.LogError("RewardedAdsManager not found.");
            //return;
        //}

        //waitingForAdReward = true;
        //ads.ShowRewardedAd(this);
    }

    public void OnRewardedAdCompleted()
    {
        if (selectedPowerUpIndex < 0)
            return;

        // Grant the exact ball whose icon was clicked.
        SpawnBall(selectedPowerUpIndex);

        selectedPowerUpIndex = -1;
        waitingForAdReward = false;

        if (watchAdPanel != null)
            watchAdPanel.SetActive(false);
    }

    public void OnRewardedAdFailedOrSkipped()
    {
        waitingForAdReward = false;
        selectedPowerUpIndex = -1;

        if (watchAdPanel != null)
            watchAdPanel.SetActive(false);

        Debug.Log("No power-up granted.");
    }

    public void OnCancelClicked()
    {
        if (waitingForAdReward)
            return;

        selectedPowerUpIndex = -1;

        if (watchAdPanel != null)
            watchAdPanel.SetActive(false);
    }
}
