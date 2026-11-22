using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight on-screen FPS and frame-time monitor for Android testing.
/// Installs itself automatically; no scene or prefab changes are required.
/// Remove this script before a public release.
/// </summary>
public sealed class PawPathFPSCounter : MonoBehaviour
{
    private const float SampleSeconds = 0.5f;

    private Canvas _canvas;
    private RectTransform _badge;
    private TMP_Text _label;
    private float _seconds;
    private int _frames;
    private int _lastWidth = -1;
    private int _lastHeight = -1;
    private float _lastScale = -1f;
    private Rect _lastSafeArea;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindAnyObjectByType<PawPathFPSCounter>() != null)
            return;

        var monitor = new GameObject("Paw Path FPS Counter");
        monitor.AddComponent<PawPathFPSCounter>();
        DontDestroyOnLoad(monitor);
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var canvasObject = new GameObject(
            "FPS Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = 200;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        _badge = new GameObject("FPS Badge", typeof(RectTransform), typeof(Image))
            .GetComponent<RectTransform>();
        _badge.SetParent(canvasObject.transform, false);
        _badge.anchorMin = Vector2.zero;
        _badge.anchorMax = Vector2.zero;
        _badge.pivot = Vector2.zero;
        _badge.sizeDelta = new Vector2(330f, 68f);

        var background = _badge.GetComponent<Image>();
        background.color = new Color32(14, 25, 45, 205);
        background.raycastTarget = false; // Never intercept gameplay taps.

        var textObject = new GameObject(
            "Readout", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(_badge, false);

        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _label = textObject.GetComponent<TextMeshProUGUI>();
        _label.text = "FPS --  |  -- ms";
        _label.fontSize = 32f;
        _label.fontStyle = FontStyles.Bold;
        _label.alignment = TextAlignmentOptions.Center;
        _label.color = Color.white;
        _label.raycastTarget = false;
    }

    private void Update()
    {
        UpdateSafeAreaPosition();

        // Unscaled time keeps the monitor active even in the pause menu.
        _seconds += Time.unscaledDeltaTime;
        _frames++;

        if (_seconds < SampleSeconds)
            return;

        float fps = _frames / Mathf.Max(_seconds, 0.0001f);
        float frameMs = 1000f * _seconds / _frames;
        _label.text = $"FPS {fps:0}  |  {frameMs:0.0} ms";
        _label.color = fps >= 50f
            ? new Color32(127, 235, 162, 255)
            : fps >= 28f
                ? new Color32(255, 218, 112, 255)
                : new Color32(255, 135, 135, 255);

        _seconds = 0f;
        _frames = 0;
    }

    private void UpdateSafeAreaPosition()
    {
        Rect safeArea = Screen.safeArea;
        float scale = Mathf.Max(0.01f, _canvas.scaleFactor);

        if (Screen.width == _lastWidth &&
            Screen.height == _lastHeight &&
            Mathf.Approximately(scale, _lastScale) &&
            safeArea.Equals(_lastSafeArea))
            return;

        _lastWidth = Screen.width;
        _lastHeight = Screen.height;
        _lastScale = scale;
        _lastSafeArea = safeArea;

        _badge.anchoredPosition = new Vector2(
            safeArea.xMin / scale + 24f,
            safeArea.yMin / scale + 24f);
    }
}
