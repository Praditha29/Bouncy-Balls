using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialTarget { None, BlueGold, GreenPurple, RedYellow, Custom }

[System.Serializable]
public class TutorialStep
{
    public string title;
    [TextArea(2, 5)] public string text;
    public TutorialTarget highlight;
    public List<GameObject> customTargets;   // only used when highlight = Custom
}

[System.Serializable]
public class SpecialBallTutorial
{
    public string id;                       // must match the trigger's ID, e.g. "Bomb"
    public string title;                    // e.g. "Bomb Ball"
    [TextArea(2, 5)] public string text;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RectTransform tutorialPanel;
    [SerializeField] private TMP_Text titleText;          // NEW: label of the highlighted ball
    [SerializeField] private TMP_Text instructionText;    // description
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;
    [SerializeField] private Button skipButton;

    [Header("World")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Collider2D playArea;

    [Header("Highlight")]
    [SerializeField] private SpriteRenderer highlightPrefab;
    [SerializeField] private float glowScale = 1.5f;      // glow size relative to the ball's on-screen size
    [SerializeField] private float pulseAmount = 0.15f;
    [SerializeField] private float pulseSpeed = 4f;

    [Header("Placement")]
    [SerializeField] private float paddingPixels = 30f;
    [SerializeField] private float searchStepPixels = 30f;

    [Header("Timing")]
    [SerializeField] private float introDelay = 0.4f;
    [SerializeField] private float introMaxWaitForBalls = 3f;   // waits until all 6 colours exist
    [SerializeField] private float specialTutorialDelay = 2f;   // delay before Bomb etc. tutorial

    [SerializeField] private float glowWorldDiameter = 0f;   // 0 = auto (largest highlighted ball x glowScale)

    [Header("Intro tutorial (plays once at game start)")]
    [SerializeField]
    private List<TutorialStep> introSteps = new List<TutorialStep>
    {
        new TutorialStep { highlight = TutorialTarget.BlueGold,
            title = "Blue & Gold Balls",
            text = "These balls spawn 4 more balls when popped." },
        new TutorialStep { highlight = TutorialTarget.GreenPurple,
            title = "Green & Purple Balls",
            text = "These balls spawn 3 more balls when popped." },
        new TutorialStep { highlight = TutorialTarget.RedYellow,
            title = "Red & Yellow Balls",
            text = "These balls spawn 2 more balls when popped." },
    };

    [Header("Special ball tutorials (play once, when the ball first spawns)")]
    [SerializeField]
    private List<SpecialBallTutorial> specialTutorials = new List<SpecialBallTutorial>
    {
        new SpecialBallTutorial { id = "Bomb",
            title = "Bomb Ball",
            text = "Tap it to destroy every ball inside its blast radius." }
    };

    [Header("Icon highlight (Custom steps)")]
    [SerializeField] private SpriteRenderer squareHighlightPrefab;   // for world-sprite icons (optional)
    [SerializeField] private float iconGlowScale = 1.25f;            // world-sprite icons
    [SerializeField] private Color iconGlowColor = new Color(1f, 0.9f, 0.2f, 0.6f);   // UI icons
    [SerializeField] private float iconGlowPaddingPixels = 12f;      // UI icons: extra size on each side
    [SerializeField] private Sprite uiGlowSprite;                    // optional, e.g. a rounded square

    private struct RuntimeStep
    {
        public string title;
        public string text;
        public TutorialTarget target;
        public GameObject specific;
        public List<GameObject> custom;     // NEW
    }

    private class Highlight
    {
        public SpriteRenderer ball;
        public SpriteRenderer glow;
        public Vector3 baseScale;

        public RectTransform uiIcon;   // UI icons
        public RectTransform uiGlow;
    }

    private readonly List<RuntimeStep> steps = new List<RuntimeStep>();
    private readonly List<Highlight> highlights = new List<Highlight>();
    private int stepIndex;
    private bool isOpen;
    private bool isPending;            // waiting for a delay before opening
    private float previousTimeScale = 1f;
    private string currentSpecialId;

    private const string IntroSeenKey = "TutorialSeen_Intro";
    private const string SpecialSeenPrefix = "TutorialSeen_";

    private void Awake()
    {
        Instance = this;
        if (gameCamera == null) gameCamera = Camera.main;
        nextButton.onClick.AddListener(OnNext);
        skipButton.onClick.AddListener(Close);
        tutorialPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(IntroSeenKey, 0) == 0)
            StartCoroutine(OpenIntroAfterDelay());
    }

    private IEnumerator OpenIntroAfterDelay()
    {
        isPending = true;
        yield return new WaitForSecondsRealtime(introDelay);

        // Wait until every colour exists (or give up after the max wait)
        float waited = 0f;
        while (!AllIntroColoursPresent() && waited < introMaxWaitForBalls)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        isPending = false;
        OpenIntro();
    }

    private bool AllIntroColoursPresent()
    {
        return Exists<BlueBall>() && Exists<GoldBall>() && Exists<GreenBall>()
            && Exists<PurpleBall>() && Exists<RedBall>() && Exists<YellowBall>();
    }

    private static bool Exists<T>() where T : MonoBehaviour
    {
        foreach (T c in FindObjectsByType<T>(FindObjectsSortMode.None))
            if (c.GetType() == typeof(T)) return true;
        return false;
    }

    // ------------------------------------------------------------------
    // PUBLIC API
    // ------------------------------------------------------------------

    public void OpenIntro()
    {
        if (isOpen) return;

        steps.Clear();
        foreach (var s in introSteps)
            steps.Add(new RuntimeStep
            {
                title = s.title,
                text = s.text,
                target = s.highlight,
                custom = s.customTargets
            });

        currentSpecialId = null;
        Begin();
    }

    public void TryShowSpecial(string id, GameObject ball)
    {
        if (isOpen || isPending) return;
        if (PlayerPrefs.GetInt(SpecialSeenPrefix + id, 0) == 1) return;

        SpecialBallTutorial info = specialTutorials.Find(t => t.id == id);
        if (info == null)
        {
            Debug.LogWarning("No special tutorial found for id: " + id);
            return;
        }

        StartCoroutine(ShowSpecialAfterDelay(info, ball));
    }

    private IEnumerator ShowSpecialAfterDelay(SpecialBallTutorial info, GameObject ball)
    {
        isPending = true;
        yield return new WaitForSeconds(specialTutorialDelay);   // game keeps running during the delay
        isPending = false;

        // Ball may have been popped/destroyed during the delay
        if (ball == null || isOpen) yield break;

        steps.Clear();
        steps.Add(new RuntimeStep { title = info.title, text = info.text, specific = ball });

        currentSpecialId = info.id;
        Begin();
    }

    // ------------------------------------------------------------------
    // FLOW
    // ------------------------------------------------------------------

    private void Begin()
    {
        isOpen = true;
        stepIndex = 0;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        tutorialPanel.gameObject.SetActive(true);
        ShowStep();
    }

    private void OnNext()
    {
        stepIndex++;
        ShowStep();
    }

    private void ShowStep()
    {
        List<GameObject> targets = null;
        while (stepIndex < steps.Count)
        {
            targets = ResolveTargets(steps[stepIndex]);
            bool needsTargets = steps[stepIndex].target != TutorialTarget.None
                                || steps[stepIndex].specific != null;
            if (!needsTargets || targets.Count > 0) break;
            stepIndex++;
        }

        if (stepIndex >= steps.Count) { Close(); return; }

        if (titleText != null) titleText.text = steps[stepIndex].title;
        instructionText.text = steps[stepIndex].text;
        nextButtonLabel.text = (stepIndex == steps.Count - 1) ? "Got it!" : "Next";

        RemoveHighlights();
        CreateHighlights(targets, steps[stepIndex].target == TutorialTarget.Custom);
        PlacePanel();
    }

    private void Close()
    {
        isOpen = false;
        RemoveHighlights();
        tutorialPanel.gameObject.SetActive(false);
        Time.timeScale = previousTimeScale;

        string key = currentSpecialId == null ? IntroSeenKey : SpecialSeenPrefix + currentSpecialId;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        currentSpecialId = null;
    }

    // ------------------------------------------------------------------
    // FINDING BALLS (exact class only, tag is never touched)
    // ------------------------------------------------------------------

    private List<GameObject> ResolveTargets(RuntimeStep step)
    {
        var list = new List<GameObject>();

        if (step.specific != null) { list.Add(step.specific); return list; }

        switch (step.target)
        {
            case TutorialTarget.BlueGold:
                AddAll<BlueBall>(list); AddAll<GoldBall>(list); break;
            case TutorialTarget.GreenPurple:
                AddAll<GreenBall>(list); AddAll<PurpleBall>(list); break;
            case TutorialTarget.RedYellow:
                AddAll<RedBall>(list); AddAll<YellowBall>(list); break;
            case TutorialTarget.Custom:
                if (step.custom != null)
                    foreach (GameObject g in step.custom)
                        if (g != null && g.activeInHierarchy && !list.Contains(g)) list.Add(g);
                break;
        }

        Debug.Log("Tutorial step '" + step.title + "' found " + list.Count + " balls to highlight.");
        return list;
    }

    private static void AddAll<T>(List<GameObject> list) where T : MonoBehaviour
    {
        foreach (T c in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            // Exact type only: ignores subclasses (special balls, shared base classes)
            if (c.GetType() != typeof(T)) continue;
            if (!list.Contains(c.gameObject)) list.Add(c.gameObject);
        }
    }

    // ------------------------------------------------------------------
    // HIGHLIGHTS
    // ------------------------------------------------------------------

    private void CreateHighlights(List<GameObject> targets, bool square)
    {
        var objs = new List<GameObject>();
        var renderers = new List<SpriteRenderer>();
        float largest = 0f;

        foreach (GameObject go in targets)
        {
            // UI icon (lives under a Canvas)
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null && go.GetComponentInParent<Canvas>() != null)
            {
                CreateUIHighlight(rt);
                continue;
            }

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) continue;

            objs.Add(go);
            renderers.Add(sr);
            largest = Mathf.Max(largest, sr.bounds.size.x, sr.bounds.size.y);
        }

        SpriteRenderer prefab = (square && squareHighlightPrefab != null) ? squareHighlightPrefab : highlightPrefab;
        float diameter = square ? largest * iconGlowScale
                                : (glowWorldDiameter > 0f ? glowWorldDiameter : largest * glowScale);

        for (int i = 0; i < objs.Count; i++)
        {
            GameObject go = objs[i];
            SpriteRenderer sr = renderers[i];
            var h = new Highlight { ball = sr };

            if (prefab != null && prefab.sprite != null)
            {
                SpriteRenderer glow = Instantiate(prefab, go.transform);
                glow.transform.position = sr.bounds.center;

                Vector3 parentScale = go.transform.lossyScale;
                Vector2 native = glow.sprite.bounds.size;

                h.baseScale = new Vector3(
                    diameter / (native.x * Mathf.Max(0.0001f, Mathf.Abs(parentScale.x))),
                    diameter / (native.y * Mathf.Max(0.0001f, Mathf.Abs(parentScale.y))),
                    1f);

                glow.transform.localScale = h.baseScale;
                glow.sortingLayerID = sr.sortingLayerID;
                glow.sortingOrder = sr.sortingOrder - 1;
                h.glow = glow;
            }

            highlights.Add(h);
        }
    }

    private void CreateUIHighlight(RectTransform icon)
    {
        var go = new GameObject("TutorialGlow", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        RectTransform rt = (RectTransform)go.transform;
        RectTransform parent = (RectTransform)icon.parent;

        rt.SetParent(parent, false);
        rt.SetSiblingIndex(icon.GetSiblingIndex());          // same index = drawn just behind the icon
        go.GetComponent<LayoutElement>().ignoreLayout = true; // layout groups won't move it

        Image img = go.GetComponent<Image>();
        img.sprite = uiGlowSprite;                            // null = plain square
        img.color = iconGlowColor;
        img.raycastTarget = false;

        // Centre the glow on the icon, whatever the icon's anchors/pivot are
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.position = icon.TransformPoint(icon.rect.center);

        Vector3 iconScale = icon.lossyScale;
        Vector3 parentScale = parent.lossyScale;
        Vector2 size = new Vector2(
            icon.rect.width * iconScale.x / Mathf.Max(0.0001f, parentScale.x),
            icon.rect.height * iconScale.y / Mathf.Max(0.0001f, parentScale.y));
        rt.sizeDelta = size + Vector2.one * iconGlowPaddingPixels * 2f;

        highlights.Add(new Highlight { uiIcon = icon, uiGlow = rt, baseScale = Vector3.one });
    }

    private void RemoveHighlights()
    {
        foreach (var h in highlights)
        {
            if (h.glow != null) Destroy(h.glow.gameObject);
            if (h.uiGlow != null) Destroy(h.uiGlow.gameObject);
        }
        highlights.Clear();
    }

    private void Update()
    {
        if (!isOpen) return;

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        foreach (var h in highlights)
        {
            if (h.glow != null) h.glow.transform.localScale = h.baseScale * pulse;
            if (h.uiGlow != null) h.uiGlow.localScale = h.baseScale * pulse;
        }
    }

    // ------------------------------------------------------------------
    // PANEL PLACEMENT
    // ------------------------------------------------------------------

    private void PlacePanel()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tutorialPanel);

        Rect area = WorldBoundsToScreenRect(playArea.bounds);
        area = Intersect(area, new Rect(0, 0, Screen.width, Screen.height));

        Vector2 size = Vector2.Scale(tutorialPanel.rect.size, tutorialPanel.lossyScale);
        Vector2 half = size * 0.5f;

        List<Rect> blocked = new List<Rect>();
        foreach (var h in highlights)
        {
            Rect r;
            if (h.uiIcon != null)
            {
                r = UIToScreenRect(h.uiGlow != null ? h.uiGlow : h.uiIcon);
            }
            else
            {
                if (h.ball == null) continue;
                Bounds b = h.glow != null ? h.glow.bounds : h.ball.bounds;
                r = WorldBoundsToScreenRect(b);
            }
            r.xMin -= paddingPixels; r.xMax += paddingPixels;
            r.yMin -= paddingPixels; r.yMax += paddingPixels;
            blocked.Add(r);
        }


        float minX = area.xMin + half.x, maxX = area.xMax - half.x;
        float minY = area.yMin + half.y, maxY = area.yMax - half.y;
        if (minX > maxX) minX = maxX = area.center.x;
        if (minY > maxY) minY = maxY = area.center.y;

        int stepsX = Mathf.Max(1, Mathf.CeilToInt((maxX - minX) / searchStepPixels));
        int stepsY = Mathf.Max(1, Mathf.CeilToInt((maxY - minY) / searchStepPixels));

        Vector2 preferred = area.center;
        Vector2 bestPos = preferred;
        float bestOverlap = float.MaxValue;
        float bestDist = float.MaxValue;

        for (int ix = 0; ix <= stepsX; ix++)
        {
            for (int iy = 0; iy <= stepsY; iy++)
            {
                Vector2 pos = new Vector2(
                    Mathf.Lerp(minX, maxX, ix / (float)stepsX),
                    Mathf.Lerp(minY, maxY, iy / (float)stepsY));

                Rect candidate = new Rect(pos - half, size);

                float overlap = 0f;
                foreach (Rect r in blocked) overlap += OverlapArea(candidate, r);

                float dist = (pos - preferred).sqrMagnitude;

                if (overlap < bestOverlap - 0.01f ||
                    (Mathf.Abs(overlap - bestOverlap) <= 0.01f && dist < bestDist))
                {
                    bestOverlap = overlap;
                    bestDist = dist;
                    bestPos = pos;
                }
            }
        }

        tutorialPanel.position = new Vector3(bestPos.x, bestPos.y, 0f);

        if (bestOverlap > 0f)
            Debug.LogWarning("Tutorial panel couldn't avoid every highlighted ball. " +
                             "Make the panel smaller or lower paddingPixels.");
    }

    private Rect WorldBoundsToScreenRect(Bounds b)
    {
        Vector3 min = gameCamera.WorldToScreenPoint(b.min);
        Vector3 max = gameCamera.WorldToScreenPoint(b.max);
        return Rect.MinMaxRect(
            Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
    }

    private static Rect Intersect(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin), xMax = Mathf.Min(a.xMax, b.xMax);
        float yMin = Mathf.Max(a.yMin, b.yMin), yMax = Mathf.Min(a.yMax, b.yMax);
        if (xMax < xMin) xMax = xMin;
        if (yMax < yMin) yMax = yMin;
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static float OverlapArea(Rect a, Rect b)
    {
        float w = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin);
        float h = Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin);
        return (w > 0f && h > 0f) ? w * h : 0f;
    }
    private Rect UIToScreenRect(RectTransform rt)
    {
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Canvas canvas = rt.GetComponentInParent<Canvas>().rootCanvas;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : (canvas.worldCamera != null ? canvas.worldCamera : gameCamera);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
        return Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
                               Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
    }
}