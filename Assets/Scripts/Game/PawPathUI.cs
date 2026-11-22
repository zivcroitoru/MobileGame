using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScreenFlow))]
public class PawPathUI : MonoBehaviour
{
    private static readonly Color Ink = new Color32(30, 42, 81, 255);
    private static readonly Color Muted = new Color32(68, 80, 119, 255);
    private static readonly Color Sky = new Color32(205, 230, 255, 255);
    private static readonly Color Gold = new Color32(255, 194, 69, 255);
    private static readonly Color GreenButton = new Color32(200, 166, 209, 255);
    private static readonly Color BlueButton = new Color32(214, 180, 233, 255);
    private static readonly Color Mint = new Color32(64, 209, 143, 255);
    private static readonly Vector2 Center = new(0.5f, 0.5f);
    private static readonly Vector2 Top = new(0.5f, 1f);
    private static readonly Vector2 TopLeft = new(0f, 1f);
    private static readonly Vector2 TopRight = Vector2.one;

    [SerializeField] private LevelGoal _goal;

    private ScreenFlow _flow;
    private RectTransform _canvas;
    private RectTransform _safeArea;
    private RectTransform _modal;
    private CoinCounter _coins;
    private TapToPush3D _push;
    private Image _progress;
    private TMP_Text _levelLabel;
    private float _startX;
    private float _bestProgress;
    private int _displayedPercent = -1;
    private bool _frozen;
    private bool _pushWasEnabled;
    private float _previousTimeScale;

    private void Start()
    {
        if (Services.Save == null || Services.Config == null || Services.Settings == null)
        {
            Debug.LogError("Paw Path UI requires services from the Bootstrap scene.", this);
            return;
        }

        _flow = GetComponent<ScreenFlow>();
        _canvas = new GameObject("Paw Path Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<RectTransform>();
        _canvas.SetParent(transform, false);
        _canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = _canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        if (_goal == null)
        {
            var background = Picture(Stretch("Home Background", _canvas), null, Sky);
            background.raycastTarget = false;
        }

        _safeArea = SafeArea(_canvas);
        if (_goal == null)
            BuildHome();
        else
            BuildHud();
    }

    private void BuildHome()
    {
        BuildWallet(_safeArea, false);

        var sound = IconButton(_safeArea, "Sound", Services.Settings.SoundOn ? "SoundOn" : "SoundOff",
            TopRight, -110, -104, out var soundIcon);
        sound.onClick.AddListener(() =>
        {
            Services.Settings.SoundOn = !Services.Settings.SoundOn;
            soundIcon.sprite = Art(Services.Settings.SoundOn ? "SoundOn" : "SoundOff");
        });

        var card = Box("Home Card", _safeArea, Center, 0, -18, 880, 1050);
        Icon(card, "Badge", Center, 0, 240, 412, 412);
        Icon(card, "Cat", Center, 0, 241, 475, 430);
        Icon(card, "Star", Center, -245, 339, 92, 92).color = Gold;
        Icon(card, "Star", Center, 245, 156, 75, 75).color = Gold;
        Label(card, "Paw Path", 118, Ink, Center, 0, -40, 800, 138);
        Label(card, $"Level {ScreenFlow.UnlockedLevel}", 44, Muted, Center, 0, -141, 710, 66);
        Action(card, "Play", "Play", true, -277, _flow.Play, 172);
        Action(card, "Upgrades", "Upgrade", false, -431, ShowUpgrades);
    }

    private void ShowUpgrades()
    {
        var card = Modal("Upgrades", 860);
        Icon(card, "Coin", Center, 0, 188, 180, 180);
        Label(card, "Reward Boost", 60, Ink, Center, 0, 72, 700, 82);
        var details = Label(card, "", 41, Muted, Center, 0, -65, 710, 167);
        details.fontStyle = FontStyles.Normal;
        // Keep the wallet above the dimmer while the shop is open.
        var previousWallet = _coins;
        BuildWallet((RectTransform)card.parent, false);

        var shop = card.gameObject.AddComponent<UpgradeShop>();
        var buy = Action(card, "Buy", "Upgrade", true, -239, shop.Buy);
        shop.Bind(buy, details, buy.GetComponentInChildren<TMP_Text>(), _coins);
        Action(card, "Back", null, false, -360, () =>
        {
            CloseModal();
            _coins = previousWallet;
            _coins.Refresh();
        });
    }

    private void BuildHud()
    {
        _startX = _goal.CatPelvis.position.x;
        _push = _goal.CatPelvis.GetComponentInParent<TapToPush3D>();
        _goal.Result.Finished += ShowResult;

        BuildWallet(_safeArea, true);
        var track = Box("Course Progress", _safeArea, Top, 55, -94, 432, 104, blocksGameplay: true);
        _levelLabel = Label(track, "", 36, Ink, Center, 0, 19, 410, 53);
        var bar = Place("Progress Track", track, Center, 0, -24, 372, 18);
        Picture(bar, "Panel", Sky);
        _progress = Picture(Stretch("Progress Fill", bar), "Panel", Mint);
        _progress.type = Image.Type.Filled;
        _progress.fillMethod = Image.FillMethod.Horizontal;
        Update();

        IconButton(_safeArea, "Pause", "Pause", TopRight, -92, -94, out _)
            .onClick.AddListener(ShowPause);
    }

    private void BuildWallet(RectTransform area, bool compact)
    {
        var wallet = Box("Wallet", area, TopLeft, compact ? 160 : 186, compact ? -94 : -104,
            compact ? 248 : 300, 104, blocksGameplay: compact);
        Icon(wallet, "Coin", Center, compact ? -76 : -94, 0, 106, 106);
        var balance = Label(wallet, "", 44, Ink, Center, 29, 0, compact ? 151 : 193, 78);
        _coins = wallet.gameObject.AddComponent<CoinCounter>();
        _coins.Bind(balance);
    }

    private void Update()
    {
        if (_progress == null || _frozen) return;

        float progress = Mathf.InverseLerp(_startX, _goal.transform.position.x, _goal.CatPelvis.position.x);
        _bestProgress = Mathf.Max(_bestProgress, Mathf.Min(0.99f, progress));
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        _progress.fillAmount = _bestProgress;
        int percent = Mathf.FloorToInt(_bestProgress * 100f);
        if (_displayedPercent == percent) return;
        _displayedPercent = percent;
        _levelLabel.text = $"Level {ScreenFlow.CurrentLevel}  •  {percent}%";
    }

    private void ShowPause()
    {
        FreezeCourse();
        var card = Modal("Paused", 580);
        Action(card, "Resume", "Play", true, 51, () =>
        {
            CloseModal();
            UnfreezeCourse();
        });
        Action(card, "Retry", "Retry", false, -78, _flow.Retry);
        Action(card, "Home", null, false, -207, _flow.Back);
    }

    private void ShowResult(bool won, int reward)
    {
        CloseModal();
        if (won)
        {
            _bestProgress = 1f;
            UpdateProgress();
        }

        FreezeCourse();
        _coins.Refresh();
        var card = Modal(won ? "Level Complete!" : "Try Again", 744);
        if (won)
            Icon(card, "Trophy", Center, 0, 124, 210, 210).color = Gold;
        Icon(card, "Coin", Center, -133, 14, 102, 102);
        Label(card, $"+{reward:N0} coins", 55, Ink, Center, 35, 14, 280, 82);

        if (won)
        {
            Action(card, "Next Level", "Next", true, -102, _flow.NextLevel);
            Action(card, "Retry", "Retry", false, -233, _flow.Retry);
        }
        else
        {
            Action(card, "Retry", "Retry", true, -102, _flow.Retry);
        }

        Action(card, "Home", null, false, won ? -327 : -233, _flow.Back, won ? 64 : 112);
    }

    private void FreezeCourse()
    {
        if (_frozen) return;
        _frozen = true;
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0;
        if (_push != null)
        {
            _pushWasEnabled = _push.enabled;
            _push.enabled = false;
        }
    }

    private void UnfreezeCourse()
    {
        if (!_frozen) return;
        _frozen = false;
        Time.timeScale = _previousTimeScale;
        if (_push != null) _push.enabled = _pushWasEnabled;
    }

    private void OnDestroy()
    {
        if (_goal != null && _goal.Result != null) _goal.Result.Finished -= ShowResult;
        UnfreezeCourse();
    }

    private RectTransform Modal(string title, float height)
    {
        _modal = Stretch(title + " Overlay", _canvas);
        var dimmer = Picture(Stretch("Dimmer", _modal), null, new Color32(16, 22, 54, 190));
        dimmer.raycastTarget = true;
        var area = SafeArea(_modal);
        var card = Box(title, area, Center, 0, 0, 860, height);
        Label(card, title, 78, Ink, Top, 0, -79, 778, 111);
        return card;
    }

    private void CloseModal()
    {
        if (_modal == null) return;
        _modal.gameObject.SetActive(false);
        Destroy(_modal.gameObject);
        _modal = null;
    }

    private static RectTransform SafeArea(RectTransform parent)
    {
        var area = Stretch("Safe Area", parent);
        area.gameObject.AddComponent<SafeAreaFitter>();
        return area;
    }

    private static RectTransform Box(string name, RectTransform parent, Vector2 anchor,
        float x, float y, float width, float height, bool blocksGameplay = false)
    {
        var box = Place(name, parent, anchor, x, y, width, height);
        Picture(Place("Shadow", box, Center, 0, -9, width, height), "Shadow", new Color32(55, 74, 132, 150));
        var surface = Picture(Stretch("Surface", box), "Panel", Color.white);
        if (blocksGameplay)
        {
            // The gesture controller checks for pointer handlers before starting a launch.
            surface.raycastTarget = true;
            box.gameObject.AddComponent<EventTrigger>();
        }
        return box;
    }

    private static Button Action(RectTransform parent, string title, string icon, bool primary,
        float y, UnityAction onClick, float height = 112)
    {
        var rect = Place(title, parent, Center, 0, y, 680, height);
        var button = ButtonOn(rect, primary ? "Primary" : "Secondary");
        if (icon != null)
            Icon(rect, icon, Center, -253, 4, 69, 69);

        Label(rect, title, height > 130 ? 88 : 55, Color.white,
            Center, icon == null ? 0 : 27, 5, icon == null ? 620 : 475, height - 12);
        button.onClick.AddListener(onClick);
        return button;
    }

    private static Button IconButton(RectTransform parent, string title, string icon,
        Vector2 anchor, float x, float y, out Image glyph)
    {
        var rect = Place(title, parent, anchor, x, y, 112, 104);
        var button = ButtonOn(rect, "Secondary");
        glyph = Icon(rect, icon, Center, 0, 4, 95, 95);
        return button;
    }

    private static Button ButtonOn(RectTransform rect, string art)
    {
        var target = Picture(rect, art, art == "Primary" ? GreenButton : BlueButton);
        target.raycastTarget = true;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = target;
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        var colors = button.colors;
        colors.pressedColor = new Color32(196, 214, 245, 255);
        colors.disabledColor = new Color32(123, 139, 162, 255);
        button.colors = colors;
        return button;
    }

    private static Image Icon(RectTransform parent, string art, Vector2 anchor,
        float x, float y, float width, float height)
    {
        var image = Picture(Place(art, parent, anchor, x, y, width, height), art, Color.white);
        image.preserveAspect = true;
        return image;
    }

    private static Image Picture(RectTransform rect, string art, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        if (art != null)
        {
            image.sprite = Art(art);
            if (image.sprite != null && image.sprite.border != Vector4.zero)
                image.type = Image.Type.Sliced;
        }

        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Sprite Art(string name) => Resources.Load<Sprite>("PawPathUI/" + name);

    private static TMP_Text Label(RectTransform parent, string text, float size, Color color,
        Vector2 anchor, float x, float y, float width, float height)
    {
        var rect = Place(text.Length == 0 ? "Label" : text, parent, anchor, x, y, width, height);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = FontStyles.Bold;
        label.enableAutoSizing = true;
        label.fontSizeMin = size * 0.65f;
        label.fontSizeMax = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform Place(string name, RectTransform parent, Vector2 anchor,
        float x, float y, float width, float height)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    private static RectTransform Stretch(string name, RectTransform parent)
    {
        var rect = Place(name, parent, Vector2.zero, 0, 0, 0, 0);
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }
}
