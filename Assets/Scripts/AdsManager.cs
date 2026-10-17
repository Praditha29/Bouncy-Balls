#pragma warning disable CS0414 // fields only used in device builds
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.LevelPlay; // older SDKs: using com.unity3d.mediation;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("LevelPlay IDs (Android)")]
    [SerializeField] private string appKey = "28cdcc6a5";
    [SerializeField] private string bannerAdUnitId = "0kc1j4cxp7p5knwn";
    [SerializeField] private string interstitialAdUnitId = "m0vffuu9tfic13py";
    [SerializeField] private string rewardedAdUnitId = "3lzq55vcrxwtqusq";

    [Header("Banner")]
    [SerializeField] private bool bannerAtTop = false;

    [Header("Interstitial rules")]
    [Tooltip("Minimum seconds between ANY two full-screen ads (rewarded counts too).")]
    [SerializeField] private float interstitialCooldown = 90f;
    [Tooltip("Show an interstitial every N game-ends (Restart / Main Menu taps).")]
    [SerializeField] private int gamesBetweenInterstitials = 2;

    [Header("Reliability")]
    [Tooltip("Seconds to wait before retrying a failed ad load.")]
    [SerializeField] private float retryDelay = 15f;
    [Tooltip("If an ad doesn't actually appear within this many seconds, treat it as failed.")]
    [SerializeField] private float showTimeout = 8f;

    [Header("Debug (development builds only)")]
    [SerializeField] private bool enableTestSuite = false;
    [SerializeField] private bool validateIntegration = false;

    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedAd;

    private bool initStarted;
    private bool initialized;

    private bool bannerWanted;
    private bool bannerLoaded;
    private bool bannerLoading;

    private bool showing;          // a full-screen ad is currently being shown
    private bool adDisplayed;      // the SDK confirmed the ad is on screen
    private bool rewardEarned;
    private Coroutine showWatchdog;

    private int gamesSinceInterstitial;
    private float lastFullscreenAdTime;

    private bool rewardedRetryQueued;
    private bool interstitialRetryQueued;
    private bool bannerRetryQueued;

    private Action pendingOnRewarded;
    private Action pendingOnFailed;
    private Action pendingAfterInterstitial;

    // SDK callbacks may arrive off the main thread; they are queued and run in Update()
    private readonly Queue<Action> mainThreadQueue = new Queue<Action>();

    // =====================================================
    //                      LIFECYCLE
    // =====================================================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // No interstitial during the first cooldown period of a session
        lastFullscreenAdTime = Time.realtimeSinceStartup;
    }

    private void Start()
    {
        if (Instance != this) return; // duplicate that is about to be destroyed

#if !UNITY_EDITOR
        if (initStarted) return;
        initStarted = true;

        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

        if (enableTestSuite && Debug.isDebugBuild)
            LevelPlay.SetMetaData("is_test_suite", "enable");

        LevelPlay.Init(appKey);
#endif
    }

    private void Update()
    {
        while (true)
        {
            Action next;
            lock (mainThreadQueue)
            {
                if (mainThreadQueue.Count == 0) break;
                next = mainThreadQueue.Dequeue();
            }
            try { next?.Invoke(); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

#if !UNITY_EDITOR
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;
        bannerAd?.DestroyAd();
#endif
        AudioListener.pause = false;
    }

    private void Enqueue(Action a)
    {
        lock (mainThreadQueue) { mainThreadQueue.Enqueue(a); }
    }

    // =====================================================
    //                         INIT
    // =====================================================
    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        Enqueue(() =>
        {
            if (initialized) return;
            initialized = true;
            Debug.Log("[Ads] LevelPlay initialized");

            SetupRewarded();
            SetupInterstitial();
            if (bannerWanted) CreateBanner(); // Game scene asked before init finished

            if (Debug.isDebugBuild)
            {
                if (validateIntegration) LevelPlay.ValidateIntegration();
                if (enableTestSuite) LevelPlay.LaunchTestSuite();
            }
        });
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogError("[Ads] Init failed: " + error);
    }

    // =====================================================
    //                        BANNER
    // =====================================================
    private void CreateBanner()
    {
        if (bannerAd != null) return;

        var position = bannerAtTop
            ? LevelPlayBannerPosition.TopCenter
            : LevelPlayBannerPosition.BottomCenter;

        var configBuilder = new LevelPlayBannerAd.Config.Builder();
        configBuilder.SetSize(LevelPlayAdSize.BANNER);
        configBuilder.SetPosition(position);
        configBuilder.SetDisplayOnLoad(true);
        configBuilder.SetRespectSafeArea(true);
        var bannerConfig = configBuilder.Build();

        bannerAd = new LevelPlayBannerAd(bannerAdUnitId, bannerConfig);

        bannerAd.OnAdLoaded += info => Enqueue(() =>
        {
            bannerLoading = false;
            bannerLoaded = true;
            // The banner auto-displays on load; hide it again if we no longer want it
            if (!bannerWanted) bannerAd?.HideAd();
        });
        bannerAd.OnAdLoadFailed += err => Enqueue(() =>
        {
            bannerLoading = false;
            Debug.LogWarning("[Ads] Banner load failed: " + err);
            if (!bannerRetryQueued)
            {
                bannerRetryQueued = true;
                StartCoroutine(RetryBannerRoutine());
            }
        });

        bannerLoading = true;
        bannerAd.LoadAd();
    }

    private IEnumerator RetryBannerRoutine()
    {
        yield return new WaitForSecondsRealtime(retryDelay * 2f);
        bannerRetryQueued = false;
        if (bannerWanted && bannerAd != null && !bannerLoaded && !bannerLoading)
        {
            bannerLoading = true;
            bannerAd.LoadAd();
        }
    }

    public void ShowBanner()
    {
        bannerWanted = true;
#if !UNITY_EDITOR
        if (!initialized) return; // CreateBanner() runs from OnInitSuccess

        if (bannerAd == null)
        {
            CreateBanner();
        }
        else if (bannerLoaded)
        {
            bannerAd.ShowAd();
        }
        else if (!bannerLoading && !bannerRetryQueued)
        {
            bannerLoading = true;
            bannerAd.LoadAd();
        }
        // else: still loading, it will display itself when ready
#endif
    }

    public void HideBanner()
    {
        bannerWanted = false;
#if !UNITY_EDITOR
        if (bannerAd != null && bannerLoaded) bannerAd.HideAd();
#endif
    }

    // =====================================================
    //                       REWARDED
    // =====================================================
    private void SetupRewarded()
    {
        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);

        rewardedAd.OnAdLoaded += info => Debug.Log("[Ads] Rewarded loaded");
        rewardedAd.OnAdLoadFailed += err => Enqueue(() =>
        {
            Debug.LogWarning("[Ads] Rewarded load failed: " + err);
            if (!rewardedRetryQueued)
            {
                rewardedRetryQueued = true;
                StartCoroutine(RetryRewardedRoutine());
            }
        });
        rewardedAd.OnAdDisplayed += info => Enqueue(() =>
        {
            if (!showing) return;
            adDisplayed = true;
            AudioListener.pause = true;
        });
        rewardedAd.OnAdRewarded += (info, reward) => Enqueue(() => rewardEarned = true);
        rewardedAd.OnAdDisplayFailed += (info, err) => Enqueue(() =>
        {
            Debug.LogWarning("[Ads] Rewarded display failed: " + err);
            FinishRewarded(false);
        });
        rewardedAd.OnAdClosed += info => Enqueue(() =>
        {
            AudioListener.pause = false; // always safe
            FinishRewarded(rewardEarned);
        });

        LoadRewarded();
    }

    private IEnumerator RetryRewardedRoutine()
    {
        yield return new WaitForSecondsRealtime(retryDelay);
        rewardedRetryQueued = false;
        LoadRewarded();
    }

    private void LoadRewarded()
    {
        if (rewardedAd == null || rewardedAd.IsAdReady()) return;
        rewardedAd.LoadAd();
    }

    public bool IsRewardedReady()
    {
#if UNITY_EDITOR
        return true;
#else
        return initialized && !showing && rewardedAd != null && rewardedAd.IsAdReady();
#endif
    }

    /// <summary>
    /// Exactly ONE of the two callbacks is always invoked (on the main thread).
    /// </summary>
    public void ShowRewardedAd(Action onRewarded, Action onFailed)
    {
#if UNITY_EDITOR
        onRewarded?.Invoke(); // simulate success in the Editor
#else
        if (showing || !IsRewardedReady())
        {
            LoadRewarded();
            onFailed?.Invoke(); // never leave the caller waiting
            return;
        }

        showing = true;
        adDisplayed = false;
        rewardEarned = false;
        pendingOnRewarded = onRewarded;
        pendingOnFailed = onFailed;

        StartShowWatchdog(true);
        rewardedAd.ShowAd();
#endif
    }

    private void FinishRewarded(bool success)
    {
        if (!showing || pendingOnRewarded == null && pendingOnFailed == null) return;

        StopShowWatchdog();
        showing = false;
        adDisplayed = false;
        AudioListener.pause = false;
        lastFullscreenAdTime = Time.realtimeSinceStartup; // delays the next interstitial

        Action cb = success ? pendingOnRewarded : pendingOnFailed;
        pendingOnRewarded = null;
        pendingOnFailed = null;
        rewardEarned = false;

        LoadRewarded(); // preload the next one
        cb?.Invoke();
    }

    // =====================================================
    //                     INTERSTITIAL
    // =====================================================
    private void SetupInterstitial()
    {
        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

        interstitialAd.OnAdLoaded += info => Debug.Log("[Ads] Interstitial loaded");
        interstitialAd.OnAdLoadFailed += err => Enqueue(() =>
        {
            Debug.LogWarning("[Ads] Interstitial load failed: " + err);
            if (!interstitialRetryQueued)
            {
                interstitialRetryQueued = true;
                StartCoroutine(RetryInterstitialRoutine());
            }
        });
        interstitialAd.OnAdDisplayed += info => Enqueue(() =>
        {
            if (!showing) return;
            adDisplayed = true;
            AudioListener.pause = true;
        });
        interstitialAd.OnAdDisplayFailed += (info, err) => Enqueue(() =>
        {
            Debug.LogWarning("[Ads] Interstitial display failed: " + err);
            FinishInterstitial(false);
        });
        interstitialAd.OnAdClosed += info => Enqueue(() =>
        {
            AudioListener.pause = false; // always safe
            FinishInterstitial(true);
        });

        LoadInterstitial();
    }

    private IEnumerator RetryInterstitialRoutine()
    {
        yield return new WaitForSecondsRealtime(retryDelay);
        interstitialRetryQueued = false;
        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        if (interstitialAd == null || interstitialAd.IsAdReady()) return;
        interstitialAd.LoadAd();
    }

    /// <summary>
    /// Use INSTEAD of loading a scene directly. Shows an interstitial if the rules
    /// allow, then runs 'next'. If no ad is shown, 'next' runs immediately.
    /// 'next' is always invoked exactly once.
    /// </summary>
    public static void ShowInterstitialThen(Action next)
    {
        if (Instance == null) { next?.Invoke(); return; }
        Instance.TryShowInterstitial(next);
    }

    private void TryShowInterstitial(Action next)
    {
        gamesSinceInterstitial++;

#if UNITY_EDITOR
        next?.Invoke();
#else
        bool due = gamesSinceInterstitial >= gamesBetweenInterstitials
                   && Time.realtimeSinceStartup - lastFullscreenAdTime >= interstitialCooldown;

        if (!due || showing || !initialized || interstitialAd == null || !interstitialAd.IsAdReady())
        {
            LoadInterstitial();
            next?.Invoke();
            return;
        }

        showing = true;
        adDisplayed = false;
        pendingAfterInterstitial = next;

        StartShowWatchdog(false);
        interstitialAd.ShowAd();
#endif
    }

    private void FinishInterstitial(bool wasShown)
    {
        if (!showing || pendingAfterInterstitial == null) return;

        StopShowWatchdog();
        showing = false;
        adDisplayed = false;
        AudioListener.pause = false;
        lastFullscreenAdTime = Time.realtimeSinceStartup;
        if (wasShown) gamesSinceInterstitial = 0;

        Action cb = pendingAfterInterstitial;
        pendingAfterInterstitial = null;

        LoadInterstitial();
        cb?.Invoke();
    }

    // =====================================================
    //            WATCHDOG (ad never appeared at all)
    // =====================================================
    private void StartShowWatchdog(bool rewarded)
    {
        StopShowWatchdog();
        showWatchdog = StartCoroutine(ShowWatchdogRoutine(rewarded));
    }

    private void StopShowWatchdog()
    {
        if (showWatchdog != null)
        {
            StopCoroutine(showWatchdog);
            showWatchdog = null;
        }
    }

    private IEnumerator ShowWatchdogRoutine(bool rewarded)
    {
        yield return new WaitForSecondsRealtime(showTimeout);
        showWatchdog = null;

        if (showing && !adDisplayed)
        {
            Debug.LogWarning("[Ads] Ad did not appear in time, treating as failed.");
            if (rewarded) FinishRewarded(false);
            else FinishInterstitial(false);
        }
    }
}