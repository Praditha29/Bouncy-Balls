using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOver : MonoBehaviour
{
    private enum State { Playing, RevivePending, ShowingCoins, GameOver }

    [Header("Game Settings")]
    [SerializeField] private BoxCollider2D playableArea;
    [SerializeField] private int maxBalls = 130;

    [Header("Revive Panel")]
    [SerializeField] private GameObject revivePanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float reviveTimeLimit = 5f;

    [Header("Coins Earned Panel")]
    [SerializeField] private GameObject coinsPanel;
    [SerializeField] private TextMeshProUGUI coinsPanelText;
    [SerializeField] private GameObject doubleCoinsButton;
    [Tooltip("Seconds before auto-advancing to Game Over. 0 = wait for Continue button only.")]
    [SerializeField] private float coinsPanelAutoAdvance = 5f;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI coinsEarnedText;

    [Header("Optional")]
    [Tooltip("Small message object shown briefly when an ad isn't available. Can be left empty.")]
    [SerializeField] private GameObject adUnavailableMessage;

    private State state = State.Playing;
    private bool reviveUsed;
    private bool adInProgress;
    private bool coinsAwarded;
    private bool coinsDoubled;
    private bool leavingScene;
    private int coinsEarned;
    private float countdownRemaining;
    private float coinsTimer;
    private Coroutine messageRoutine;

    private AudioManager audioManager;
    private CollectibleSpawner spawner;

    private void Awake()
    {
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
            audioManager = audioObject.GetComponent<AudioManager>();

        spawner = FindFirstObjectByType<CollectibleSpawner>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (revivePanel != null) revivePanel.SetActive(false);
        if (coinsPanel != null) coinsPanel.SetActive(false);
        if (adUnavailableMessage != null) adUnavailableMessage.SetActive(false);

        if (AdsManager.Instance != null) AdsManager.Instance.ShowBanner();
    }

    private void Update()
    {
        switch (state)
        {
            case State.Playing:
                CheckGameOver();
                break;

            case State.RevivePending:
                if (adInProgress) return;

                countdownRemaining -= Time.unscaledDeltaTime;
                if (countdownText != null)
                    countdownText.text = Mathf.CeilToInt(Mathf.Max(0f, countdownRemaining)).ToString();

                if (countdownRemaining <= 0f)
                    ShowCoinsPanel();
                break;

            case State.ShowingCoins:
                if (adInProgress) return; // pause auto-advance while an ad plays

                if (coinsPanelAutoAdvance > 0f)
                {
                    coinsTimer -= Time.unscaledDeltaTime;
                    if (coinsTimer <= 0f)
                        ShowGameOverPanel();
                }
                break;
        }
    }

    private void CheckGameOver()
    {
        if (playableArea == null) return;

        int ballLayer = LayerMask.NameToLayer("Ball");
        if (ballLayer == -1) return;

        Collider2D[] objectsInside = Physics2D.OverlapBoxAll(
            playableArea.bounds.center, playableArea.bounds.size, 0f);

        int ballCount = 0;
        foreach (Collider2D obj in objectsInside)
            if (obj.gameObject.layer == ballLayer)
                ballCount++;

        if (ballCount >= maxBalls)
            Lost();
    }

    // ---------- LOST -> REVIVE PANEL ----------
    public void Lost()
    {
        if (state != State.Playing) return;

        // Hide the banner so it can't cover the revive/coins/game over buttons
        if (AdsManager.Instance != null) AdsManager.Instance.HideBanner();

        if (audioManager != null)
            audioManager.PlaySFX(audioManager.GameOverAudio);

        // Offer revive only if unused AND a rewarded ad is actually ready
        bool canOfferRevive = !reviveUsed
                              && revivePanel != null
                              && AdsManager.Instance != null
                              && AdsManager.Instance.IsRewardedReady();

        if (canOfferRevive)
        {
            state = State.RevivePending;
            countdownRemaining = reviveTimeLimit;
            Time.timeScale = 0f;

            revivePanel.SetActive(true);
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(reviveTimeLimit).ToString();
            return;
        }

        ShowCoinsPanel();
    }

    // Hook to "Watch Ad to Revive" button
    public void WatchAdToRevive()
    {
        if (state != State.RevivePending || adInProgress) return;

        if (AdsManager.Instance == null)
        {
            Debug.LogWarning("No AdsManager found! Start the game from the MainMenu scene.");
            ShowAdUnavailable();
            return;
        }

        adInProgress = true;
        AdsManager.Instance.ShowRewardedAd(OnReviveAdRewarded, OnAdFailed);
    }

    private void OnReviveAdRewarded()
    {
        adInProgress = false;
        if (state != State.RevivePending) return; // state changed while the ad was up

        if (revivePanel != null) revivePanel.SetActive(false);
        StartCoroutine(ReviveRoutine());
    }

    private void OnAdFailed()
    {
        adInProgress = false; // timers resume; player can retry or decline
        ShowAdUnavailable();
    }

    private IEnumerator ReviveRoutine()
    {
        reviveUsed = true;

        if (spawner != null)
            spawner.PrepareForRevive();

        yield return null;

        state = State.Playing;
        Time.timeScale = 1f;

        // Bring the banner back now that gameplay resumes
        if (AdsManager.Instance != null) AdsManager.Instance.ShowBanner();
    }

    // Hook to "No Thanks" button
    public void DeclineRevive()
    {
        if (state != State.RevivePending || adInProgress) return;
        ShowCoinsPanel();
    }

    // ---------- COINS EARNED PANEL ----------
    private void ShowCoinsPanel()
    {
        state = State.ShowingCoins;
        Time.timeScale = 0f;

        if (revivePanel != null) revivePanel.SetActive(false);

        if (!coinsAwarded)
        {
            coinsAwarded = true;
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;

            if (CoinManager.Instance != null)
                coinsEarned = CoinManager.Instance.AwardRunCoins(score);
        }

        if (coinsPanelText != null)
            coinsPanelText.text = "+" + coinsEarned;

        // Offer doubling only if there is something to double AND an ad is ready
        if (doubleCoinsButton != null)
        {
            bool adReady = AdsManager.Instance != null && AdsManager.Instance.IsRewardedReady();
            doubleCoinsButton.SetActive(coinsEarned > 0 && !coinsDoubled && adReady);
        }

        if (coinsPanel == null)
        {
            ShowGameOverPanel();
            return;
        }

        coinsPanel.SetActive(true);
        coinsTimer = coinsPanelAutoAdvance;
    }

    // Hook to "Double Coins (Watch Ad)" button
    public void WatchAdToDoubleCoins()
    {
        if (state != State.ShowingCoins || adInProgress || coinsDoubled || coinsEarned <= 0)
            return;

        if (AdsManager.Instance == null)
        {
            Debug.LogWarning("No AdsManager found! Start the game from the MainMenu scene.");
            ShowAdUnavailable();
            return;
        }

        adInProgress = true;
        AdsManager.Instance.ShowRewardedAd(OnDoubleAdRewarded, OnAdFailed);
    }

    private void OnDoubleAdRewarded()
    {
        adInProgress = false;
        if (coinsDoubled) return;
        coinsDoubled = true;

        // Add the same amount again so the total is doubled
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoins(coinsEarned);

        coinsEarned *= 2;

        if (coinsPanelText != null)
            coinsPanelText.text = "+" + coinsEarned;

        if (doubleCoinsButton != null)
            doubleCoinsButton.SetActive(false);

        // If the panel already moved on (auto-advance), refresh the game over text
        if (state == State.GameOver && coinsEarnedText != null)
            coinsEarnedText.text = coinsEarned.ToString();

        // Give the player a moment to see the new amount
        coinsTimer = Mathf.Max(coinsTimer, 2f);
    }

    // Hook to the coins panel "Continue" button
    public void OnCoinsContinue()
    {
        if (state != State.ShowingCoins || adInProgress) return;
        ShowGameOverPanel();
    }

    // ---------- GAME OVER PANEL ----------
    private void ShowGameOverPanel()
    {
        state = State.GameOver;
        Time.timeScale = 0f;

        if (coinsPanel != null) coinsPanel.SetActive(false);

        int finalScore = 0, highScore = 0;
        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.GetScore();
            highScore = ScoreManager.Instance.GetHighScore();
        }

        if (finalScoreText != null) finalScoreText.text = finalScore.ToString();
        if (highScoreText != null) highScoreText.text = highScore.ToString();
        if (coinsEarnedText != null) coinsEarnedText.text = coinsEarned.ToString();

        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
    }

    public bool IsGameOver() => state != State.Playing;

    // ---------- "AD NOT AVAILABLE" MESSAGE ----------
    private void ShowAdUnavailable()
    {
        if (adUnavailableMessage == null) return;

        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(AdUnavailableRoutine());
    }

    private IEnumerator AdUnavailableRoutine()
    {
        adUnavailableMessage.SetActive(true);
        yield return new WaitForSecondsRealtime(2f); // works while timeScale = 0
        adUnavailableMessage.SetActive(false);
        messageRoutine = null;
    }

    // ---------- SCENE CHANGES (with interstitial) ----------
    public void Restart()
    {
        if (leavingScene || adInProgress) return;
        leavingScene = true;

        AdsManager.ShowInterstitialThen(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Game");
        });
    }

    public void MainMenu()
    {
        if (leavingScene || adInProgress) return;
        leavingScene = true;

        AdsManager.ShowInterstitialThen(() =>
        {
            Time.timeScale = 1f;
            if (AdsManager.Instance != null) AdsManager.Instance.HideBanner();
            SceneManager.LoadScene("MainMenu");
        });
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}