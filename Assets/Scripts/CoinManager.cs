using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Reward Rules")]
    [SerializeField] private int ballsPerReward = 50;
    [SerializeField] private int coinsPerReward = 10;

    private const string CoinsKey = "TotalCoins";

    public int TotalCoins { get; private set; }
    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        TotalCoins = PlayerPrefs.GetInt(CoinsKey, 0);
    }

    // 50 balls = 10 coins. 120 balls -> 2 full groups -> 20 coins.
    public int CalculateRunCoins(int ballsPopped)
    {
        return (ballsPopped / ballsPerReward) * coinsPerReward;
    }

    public int AwardRunCoins(int ballsPopped)
    {
        int earned = CalculateRunCoins(ballsPopped);
        AddCoins(earned);
        return earned;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        TotalCoins += amount;
        Save();
    }

    public bool SpendCoins(int amount)
    {
        if (amount > TotalCoins) return false;
        TotalCoins -= amount;
        Save();
        return true;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(CoinsKey, TotalCoins);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke(TotalCoins);
    }
}