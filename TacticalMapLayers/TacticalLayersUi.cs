using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace TacticalMapLayers
{
    /// <summary>Centered tactical layers panel: Tactical / Front / Settings; flat style (pre–Win11 look).</summary>
    public sealed class TacticalLayersUi
    {
        private static readonly Color32 PanelBg = new Color32(12, 14, 18, 240);
        private static readonly Color32 SheetBg = new Color32(18, 20, 28, 248);
        private static readonly Color32 TabIdle = new Color32(34, 38, 48, 245);
        private static readonly Color32 TabSelected = new Color32(32, 72, 108, 250);
        private static readonly Color32 AccentLine = new Color32(235, 185, 48, 245);
        private static readonly Color32 SectionMuted = new Color32(160, 168, 188, 255);
        private static readonly Color32 CheckboxBorder = new Color32(55, 58, 68, 255);
        private static readonly Color32 CheckboxFill = new Color32(235, 185, 48, 255);

        private const float PanelW = 380f;
        private const float PanelH = 292f;
        private const float SheetPad = 14f;

        private enum MainTab
        {
            Tactical,
            FrontLine,
            Settings,
        }

        private readonly TacticalLayerConfig _cfg;
        private readonly ConfigFile _config;
        private readonly ManualLogSource _log;
        private RectTransform _panelRoot;
        private RectTransform _tacticalSheet;
        private RectTransform _frontSheet;
        private RectTransform _settingsSheet;
        private MainTab _mainTab;
        private Image[] _tabImages;
        private Text[] _tabTexts;
        private Text _titleText;
        private Text _sectionTactical;
        private Text _sectionFront;
        private Text _sectionSettings;
        private Text _sectionAbout;
        private Text[] _tacticalRowLabels;
        private Text _frontLineLabel;
        private Text _frontHintText;
        private GameObject _frontHintGo;
        private Text _settingsLangCaption;
        private Text _settingsAboutText;
        private Image _langEnChip;
        private Image _langRuChip;
        private Text _hintsToggleLabel;
        private Text _resetButtonLabel;
        private Toggle[] _layerToggles;
        private Toggle _hintsToggle;

        public bool TacticalPanelVisible =>
            _panelRoot != null && _panelRoot.gameObject.activeSelf && DynamicMap.mapMaximized;

        public bool DrawEnemyGroundRadars => _cfg.LayerEnemyGroundRadars.Value;
        public bool DrawAllyStationaryRadars => _cfg.LayerAllyStationaryRadars.Value;
        public bool DrawAllyAllRadars => _cfg.LayerAllyAllRadars.Value;
        public bool DrawEnemyAirbases => _cfg.LayerEnemyAirbases.Value;
        public bool DrawFrontLine => _cfg.LayerFrontLine.Value;

        public bool AnyOverlayLayerEnabled =>
            DrawEnemyGroundRadars || DrawAllyStationaryRadars || DrawAllyAllRadars || DrawEnemyAirbases || DrawFrontLine;

        public TacticalLayersUi(TacticalLayerConfig cfg, ConfigFile config, ManualLogSource log)
        {
            _cfg = cfg;
            _config = config;
            _log = log;
        }

        public void BuildOrMove(DynamicMap map)
        {
            if (_panelRoot != null)
                return;

            SyncLangFromConfig();

            var mapCanvas = map.iconLayer.GetComponentInParent<Canvas>();
            if (mapCanvas == null)
            {
                _log.LogWarning("TacticalMapLayers: no Canvas above iconLayer.");
                return;
            }

            var root = new GameObject("TacticalMapLayers_UI", typeof(RectTransform), typeof(Image));
            _panelRoot = root.GetComponent<RectTransform>();
            var rootBg = root.GetComponent<Image>();
            rootBg.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            rootBg.type = Image.Type.Simple;
            rootBg.color = PanelBg;
            rootBg.raycastTarget = true;

            var canvasRt = mapCanvas.transform as RectTransform;
            _panelRoot.SetParent(canvasRt, false);
            _panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            _panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _panelRoot.pivot = new Vector2(0.5f, 0.5f);
            _panelRoot.anchoredPosition = Vector2.zero;
            _panelRoot.sizeDelta = new Vector2(PanelW, PanelH);

            var font = ResolveUiFont();

            const float tabTop = -6f;
            const float titleY = -46f;
            const float topInset = 78f;

            AddTabRow(font, tabTop);
            MkTitleRow(_panelRoot, font, titleY);
            _layerToggles = new Toggle[5];
            _tacticalRowLabels = new Text[4];
            _tacticalSheet = CreateTacticalSheet(font, SheetPad, topInset);
            _frontSheet = CreateFrontSheet(font, SheetPad, topInset);
            _settingsSheet = CreateSettingsSheet(font, SheetPad, topInset);
            MkAccentBar(_panelRoot);
            _panelRoot.SetAsLastSibling();
            _panelRoot.gameObject.SetActive(false);
            SelectMainTab(LoadSavedMainTab(), force: true);
            ApplyLocalization();
            ApplyFrontHintVisibility();
        }

        private void SyncLangFromConfig()
        {
            TacticalStrings.Language = TacticalStrings.Parse(_cfg.UiLanguage.Value);
        }

        private static Font ResolveUiFont()
        {
            var font = Font.CreateDynamicFontFromOSFont("Segoe UI", 15);
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont("Arial", 15);
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont("Tahoma", 15);
            return font;
        }

        private static void MkAccentBar(RectTransform parent)
        {
            var go = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.SetAsFirstSibling();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 3f);
            rt.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            img.type = Image.Type.Simple;
            img.color = AccentLine;
            img.raycastTarget = false;
        }

        private void MkTitleRow(RectTransform parent, Font font, float y)
        {
            var row = Row(parent, "TitleRow", y, 24f);
            var txtGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            var trt = txtGo.GetComponent<RectTransform>();
            trt.SetParent(row, false);
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(14f, 0f);
            trt.offsetMax = new Vector2(-12f, 0f);
            _titleText = txtGo.GetComponent<Text>();
            _titleText.font = font;
            _titleText.fontSize = 15;
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.alignment = TextAnchor.MiddleLeft;
            _titleText.color = Color.white;
            var outline = txtGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        public void OnHotkeyToggleLayersPanel()
        {
            if (_panelRoot == null || !DynamicMap.mapMaximized)
                return;

            if (!_panelRoot.gameObject.activeSelf)
            {
                _panelRoot.gameObject.SetActive(true);
                SelectMainTab(LoadSavedMainTab(), force: true);
            }
            else
            {
                _panelRoot.gameObject.SetActive(false);
            }
        }

        private void AddTabRow(Font font, float y)
        {
            var row = Row(_panelRoot, "TabRow", y, 36f);
            var tabBarBg = row.gameObject.AddComponent<Image>();
            tabBarBg.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            tabBarBg.type = Image.Type.Simple;
            tabBarBg.color = new Color32(14, 16, 20, 250);
            tabBarBg.raycastTarget = false;

            _tabTexts = new Text[3];
            _tabImages = new Image[3];
            string[] labels = { TacticalStrings.TabTactical, TacticalStrings.TabFront, TacticalStrings.TabSettings };
            MainTab[] tabs = { MainTab.Tactical, MainTab.FrontLine, MainTab.Settings };
            for (int i = 0; i < labels.Length; i++)
            {
                int idx = i;
                var btn = MkTabButton(row, labels[i], font, i, labels.Length);
                _tabImages[i] = btn.GetComponent<Image>();
                _tabTexts[i] = btn.GetComponentInChildren<Text>();
                btn.onClick.AddListener(() => SelectMainTab(tabs[idx], force: false));
            }
        }

        private void SelectMainTab(MainTab tab, bool force)
        {
            if (!DynamicMap.mapMaximized)
                return;
            if (!force && _panelRoot != null && !_panelRoot.gameObject.activeSelf)
                return;

            _mainTab = tab;
            if (_tacticalSheet != null)
                _tacticalSheet.gameObject.SetActive(tab == MainTab.Tactical);
            if (_frontSheet != null)
                _frontSheet.gameObject.SetActive(tab == MainTab.FrontLine);
            if (_settingsSheet != null)
                _settingsSheet.gameObject.SetActive(tab == MainTab.Settings);
            if (_panelRoot != null)
                _panelRoot.SetAsLastSibling();
            UpdateTabVisuals();

            if (!force && _cfg.LayersPanelMainTab != null && _cfg.LayersPanelMainTab.Value != (int)tab)
            {
                _cfg.LayersPanelMainTab.Value = (int)tab;
                TrySaveConfig();
            }
        }

        private MainTab LoadSavedMainTab()
        {
            int v = _cfg.LayersPanelMainTab != null ? _cfg.LayersPanelMainTab.Value : 0;
            if (v < 0)
                v = 0;
            if (v > 2)
                v = 2;
            return (MainTab)v;
        }

        private void UpdateTabVisuals()
        {
            if (_tabImages == null)
                return;
            for (int i = 0; i < _tabImages.Length; i++)
            {
                bool sel = (_mainTab == MainTab.Tactical && i == 0)
                    || (_mainTab == MainTab.FrontLine && i == 1)
                    || (_mainTab == MainTab.Settings && i == 2);
                _tabImages[i].color = sel ? TabSelected : TabIdle;
            }
        }

        private RectTransform CreateTacticalSheet(Font font, float pad, float topInset)
        {
            var sheet = new GameObject("TacticalSheet", typeof(RectTransform), typeof(Image));
            var rt = sheet.GetComponent<RectTransform>();
            rt.SetParent(_panelRoot, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(pad, 12f);
            rt.offsetMax = new Vector2(-pad, -topInset);
            var sheetBg = sheet.GetComponent<Image>();
            sheetBg.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            sheetBg.type = Image.Type.Simple;
            sheetBg.color = SheetBg;
            sheetBg.raycastTarget = true;

            _sectionTactical = MkSectionLabel(rt, font, -8f, TacticalStrings.SectionRadars);

            var entries = new[]
            {
                _cfg.LayerEnemyGroundRadars,
                _cfg.LayerAllyStationaryRadars,
                _cfg.LayerAllyAllRadars,
                _cfg.LayerEnemyAirbases
            };

            var startLabels = new[]
            {
                TacticalStrings.LayerEnemyGround,
                TacticalStrings.LayerAllyStationary,
                TacticalStrings.LayerAllyAll,
                TacticalStrings.LayerEnemyAirbases,
            };

            float y = -30f;
            for (int i = 0; i < entries.Length; i++)
            {
                var row = Row(rt, "Layer_" + i, y, 28f);
                var tg = MkToggle(row, startLabels[i], font, entries[i].Value, out var label);
                _tacticalRowLabels[i] = label;
                int j = i;
                tg.isOn = entries[i].Value;
                tg.onValueChanged.AddListener(v =>
                {
                    entries[j].Value = v;
                    TrySaveConfig();
                });
                _layerToggles[i] = tg;
                y -= 30f;
            }

            return rt;
        }

        private RectTransform CreateFrontSheet(Font font, float pad, float topInset)
        {
            var sheet = new GameObject("FrontLineSheet", typeof(RectTransform), typeof(Image));
            var rt = sheet.GetComponent<RectTransform>();
            rt.SetParent(_panelRoot, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(pad, 12f);
            rt.offsetMax = new Vector2(-pad, -topInset);
            var sheetBg = sheet.GetComponent<Image>();
            sheetBg.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            sheetBg.type = Image.Type.Simple;
            sheetBg.color = SheetBg;
            sheetBg.raycastTarget = true;

            _sectionFront = MkSectionLabel(rt, font, -8f, TacticalStrings.SectionAnalysis);

            var row = Row(rt, "FrontToggle", -32f, 28f);
            var tg = MkToggle(row, TacticalStrings.LayerFrontLine, font, _cfg.LayerFrontLine.Value, out _frontLineLabel);
            tg.isOn = _cfg.LayerFrontLine.Value;
            tg.onValueChanged.AddListener(v =>
            {
                _cfg.LayerFrontLine.Value = v;
                TrySaveConfig();
            });
            _layerToggles[4] = tg;

            var hint = new GameObject("FrontHint", typeof(RectTransform), typeof(Text));
            _frontHintGo = hint;
            var hrt = hint.GetComponent<RectTransform>();
            hrt.SetParent(rt, false);
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.anchoredPosition = new Vector2(0f, -66f);
            hrt.sizeDelta = new Vector2(-24f, 48f);
            _frontHintText = hint.GetComponent<Text>();
            _frontHintText.font = font;
            _frontHintText.fontSize = 12;
            _frontHintText.color = new Color(0.62f, 0.66f, 0.74f, 1f);
            _frontHintText.alignment = TextAnchor.UpperLeft;
            _frontHintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _frontHintText.verticalOverflow = VerticalWrapMode.Truncate;

            rt.gameObject.SetActive(false);
            return rt;
        }

        private RectTransform CreateSettingsSheet(Font font, float pad, float topInset)
        {
            var sheet = new GameObject("SettingsSheet", typeof(RectTransform), typeof(Image));
            var rt = sheet.GetComponent<RectTransform>();
            rt.SetParent(_panelRoot, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(pad, 12f);
            rt.offsetMax = new Vector2(-pad, -topInset);
            var sheetBg = sheet.GetComponent<Image>();
            sheetBg.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            sheetBg.type = Image.Type.Simple;
            sheetBg.color = SheetBg;
            sheetBg.raycastTarget = true;

            _sectionSettings = MkSectionLabel(rt, font, -8f, TacticalStrings.SectionInterface);

            float y = -30f;
            var langRow = Row(rt, "LangRow", y, 32f);
            var capGo = new GameObject("LangCaption", typeof(RectTransform), typeof(Text));
            var capRt = capGo.GetComponent<RectTransform>();
            capRt.SetParent(langRow, false);
            capRt.anchorMin = new Vector2(0f, 0f);
            capRt.anchorMax = new Vector2(0.5f, 1f);
            capRt.offsetMin = new Vector2(4f, 0f);
            capRt.offsetMax = new Vector2(-4f, 0f);
            _settingsLangCaption = capGo.GetComponent<Text>();
            _settingsLangCaption.font = font;
            _settingsLangCaption.fontSize = 13;
            _settingsLangCaption.alignment = TextAnchor.MiddleLeft;
            _settingsLangCaption.color = new Color(0.93f, 0.94f, 0.96f, 1f);

            var enBtn = MkLangChip(langRow, TacticalStrings.LabelLangEn, font, 0.52f, 0.70f);
            var ruBtn = MkLangChip(langRow, TacticalStrings.LabelLangRu, font, 0.71f, 0.97f);
            _langEnChip = enBtn.GetComponent<Image>();
            _langRuChip = ruBtn.GetComponent<Image>();
            enBtn.onClick.AddListener(() => SetUiLanguage(TacticalUiLanguage.English));
            ruBtn.onClick.AddListener(() => SetUiLanguage(TacticalUiLanguage.Russian));

            y -= 36f;
            var hintsRow = Row(rt, "HintsRow", y, 28f);
            _hintsToggle = MkToggle(hintsRow, TacticalStrings.LabelShowHints, font, _cfg.ShowFrontLineHint.Value, out _hintsToggleLabel);
            _hintsToggle.isOn = _cfg.ShowFrontLineHint.Value;
            _hintsToggle.onValueChanged.AddListener(v =>
            {
                _cfg.ShowFrontLineHint.Value = v;
                ApplyFrontHintVisibility();
                TrySaveConfig();
            });

            y -= 34f;
            var resetBtn = MkTextButton(Row(rt, "ResetRow", y, 30f), font, () =>
            {
                _cfg.LayerEnemyGroundRadars.Value = false;
                _cfg.LayerAllyStationaryRadars.Value = false;
                _cfg.LayerAllyAllRadars.Value = false;
                _cfg.LayerEnemyAirbases.Value = false;
                _cfg.LayerFrontLine.Value = false;
                for (int i = 0; i < _layerToggles.Length; i++)
                {
                    if (_layerToggles[i] != null)
                        _layerToggles[i].isOn = false;
                }

                TrySaveConfig();
            });
            _resetButtonLabel = resetBtn.GetComponentInChildren<Text>();

            y -= 42f;
            _sectionAbout = MkSectionLabel(rt, font, y, TacticalStrings.SectionAbout);
            y -= 20f;
            var aboutGo = new GameObject("About", typeof(RectTransform), typeof(Text));
            var art = aboutGo.GetComponent<RectTransform>();
            art.SetParent(rt, false);
            art.anchorMin = new Vector2(0f, 1f);
            art.anchorMax = new Vector2(1f, 1f);
            art.pivot = new Vector2(0.5f, 1f);
            art.anchoredPosition = new Vector2(0f, y);
            art.sizeDelta = new Vector2(-20f, 40f);
            _settingsAboutText = aboutGo.GetComponent<Text>();
            _settingsAboutText.font = font;
            _settingsAboutText.fontSize = 11;
            _settingsAboutText.color = new Color(0.55f, 0.58f, 0.65f, 1f);
            _settingsAboutText.alignment = TextAnchor.UpperLeft;
            _settingsAboutText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _settingsAboutText.verticalOverflow = VerticalWrapMode.Truncate;

            rt.gameObject.SetActive(false);
            return rt;
        }

        private void SetUiLanguage(TacticalUiLanguage lang)
        {
            _cfg.UiLanguage.Value = TacticalStrings.Code(lang);
            TacticalStrings.Language = lang;
            TrySaveConfig();
            ApplyLocalization();
            UpdateLanguageChips();
        }

        private void UpdateLanguageChips()
        {
            if (_langEnChip == null || _langRuChip == null)
                return;
            bool en = TacticalStrings.Language == TacticalUiLanguage.English;
            _langEnChip.color = en ? TabSelected : TabIdle;
            _langRuChip.color = !en ? TabSelected : TabIdle;
        }

        private static Button MkLangChip(RectTransform row, string label, Font font, float xMin, float xMax)
        {
            var go = new GameObject("Chip_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(row, false);
            rt.anchorMin = new Vector2(xMin, 0.08f);
            rt.anchorMax = new Vector2(xMax, 0.92f);
            rt.offsetMin = new Vector2(2f, 0f);
            rt.offsetMax = new Vector2(-2f, 0f);
            var img = go.GetComponent<Image>();
            img.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            img.type = Image.Type.Simple;
            img.color = TabIdle;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var trt = txtGo.GetComponent<RectTransform>();
            trt.SetParent(rt, false);
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var te = txtGo.GetComponent<Text>();
            te.font = font;
            te.fontSize = 12;
            te.fontStyle = FontStyle.Bold;
            te.alignment = TextAnchor.MiddleCenter;
            te.color = new Color(0.94f, 0.95f, 0.97f, 1f);
            te.text = label;
            return btn;
        }

        private static Button MkTextButton(RectTransform row, Font font, Action onClick)
        {
            var go = new GameObject("TextBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(row, false);
            rt.anchorMin = new Vector2(0.04f, 0.18f);
            rt.anchorMax = new Vector2(0.96f, 0.82f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            img.type = Image.Type.Simple;
            img.color = new Color32(40, 44, 54, 255);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.75f, 0.76f, 0.8f, 1f);
            colors.pressedColor = new Color(0.55f, 0.56f, 0.6f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick());

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var trt = txtGo.GetComponent<RectTransform>();
            trt.SetParent(rt, false);
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(8f, 0f);
            trt.offsetMax = new Vector2(-8f, 0f);
            var te = txtGo.GetComponent<Text>();
            te.font = font;
            te.fontSize = 13;
            te.alignment = TextAnchor.MiddleCenter;
            te.color = new Color(0.93f, 0.94f, 0.96f, 1f);
            return btn;
        }

        private void ApplyLocalization()
        {
            SyncLangFromConfig();
            if (_titleText != null)
                _titleText.text = TacticalStrings.WindowTitle;
            if (_tabTexts != null && _tabTexts.Length >= 3)
            {
                _tabTexts[0].text = TacticalStrings.TabTactical;
                _tabTexts[1].text = TacticalStrings.TabFront;
                _tabTexts[2].text = TacticalStrings.TabSettings;
            }

            if (_sectionTactical != null)
                _sectionTactical.text = TacticalStrings.SectionRadars.ToUpperInvariant();
            if (_sectionFront != null)
                _sectionFront.text = TacticalStrings.SectionAnalysis.ToUpperInvariant();
            if (_sectionSettings != null)
                _sectionSettings.text = TacticalStrings.SectionInterface.ToUpperInvariant();
            if (_sectionAbout != null)
                _sectionAbout.text = TacticalStrings.SectionAbout.ToUpperInvariant();

            if (_tacticalRowLabels != null && _tacticalRowLabels.Length >= 4)
            {
                _tacticalRowLabels[0].text = TacticalStrings.LayerEnemyGround;
                _tacticalRowLabels[1].text = TacticalStrings.LayerAllyStationary;
                _tacticalRowLabels[2].text = TacticalStrings.LayerAllyAll;
                _tacticalRowLabels[3].text = TacticalStrings.LayerEnemyAirbases;
            }

            if (_frontLineLabel != null)
                _frontLineLabel.text = TacticalStrings.LayerFrontLine;
            if (_frontHintText != null)
                _frontHintText.text = TacticalStrings.FrontHint;

            if (_settingsLangCaption != null)
                _settingsLangCaption.text = TacticalStrings.LabelLanguage;
            if (_hintsToggleLabel != null)
                _hintsToggleLabel.text = TacticalStrings.LabelShowHints;
            if (_resetButtonLabel != null)
                _resetButtonLabel.text = TacticalStrings.LabelResetLayers;
            if (_settingsAboutText != null)
            {
                _settingsAboutText.text = TacticalMapLayersPlugin.PluginName + " " + TacticalMapLayersPlugin.PluginVersion
                    + (TacticalStrings.Language == TacticalUiLanguage.English
                        ? " — client-side map overlays."
                        : " — оверлеи карты только на клиенте.");
            }

            UpdateLanguageChips();
        }

        private void ApplyFrontHintVisibility()
        {
            if (_frontHintGo != null)
                _frontHintGo.SetActive(_cfg.ShowFrontLineHint.Value);
        }

        private static Text MkSectionLabel(RectTransform sheet, Font font, float y, string text)
        {
            var row = Row(sheet, "Section_" + y, y, 20f);
            var txtGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var trt = txtGo.GetComponent<RectTransform>();
            trt.SetParent(row, false);
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(2f, 0f);
            trt.offsetMax = Vector2.zero;
            var te = txtGo.GetComponent<Text>();
            te.font = font;
            te.fontStyle = FontStyle.Bold;
            te.fontSize = 11;
            te.alignment = TextAnchor.MiddleLeft;
            te.color = SectionMuted;
            te.text = text.ToUpperInvariant();
            return te;
        }

        private void TrySaveConfig()
        {
            try
            {
                _config.Save();
            }
            catch (Exception ex)
            {
                _log.LogWarning("Config save: " + ex.Message);
            }
        }

        public void OnMapMaximizedChanged(bool maximized)
        {
            if (!maximized)
            {
                if (_panelRoot != null)
                    _panelRoot.gameObject.SetActive(false);
                _mainTab = MainTab.Tactical;
                if (_tacticalSheet != null)
                    _tacticalSheet.gameObject.SetActive(false);
                if (_frontSheet != null)
                    _frontSheet.gameObject.SetActive(false);
                if (_settingsSheet != null)
                    _settingsSheet.gameObject.SetActive(false);
                UpdateTabVisuals();
            }
        }

        public void Destroy()
        {
            _mainTab = MainTab.Tactical;
            _tabImages = null;
            _tabTexts = null;
            _layerToggles = null;
            if (_panelRoot != null)
            {
                UnityEngine.Object.Destroy(_panelRoot.gameObject);
                _panelRoot = null;
                _tacticalSheet = null;
                _frontSheet = null;
                _settingsSheet = null;
            }
        }

        private static RectTransform Row(RectTransform parent, string name, float y, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(0f, height);
            return rt;
        }

        private static Button MkTabButton(RectTransform row, string text, Font font, int columnIndex, int columns)
        {
            var go = new GameObject("Btn_" + columnIndex, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(row, false);
            float w = 1f / columns;
            rt.anchorMin = new Vector2(columnIndex * w, 0f);
            rt.anchorMax = new Vector2((columnIndex + 1) * w, 1f);
            rt.offsetMin = new Vector2(4f, 4f);
            rt.offsetMax = new Vector2(-4f, -4f);
            var img = go.GetComponent<Image>();
            img.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            img.type = Image.Type.Simple;
            img.color = TabIdle;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var trt = txtGo.GetComponent<RectTransform>();
            trt.SetParent(rt, false);
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var te = txtGo.GetComponent<Text>();
            te.font = font;
            te.fontSize = 12;
            te.alignment = TextAnchor.MiddleCenter;
            te.color = new Color(0.94f, 0.95f, 0.97f, 1f);
            te.text = text;
            return btn;
        }

        private static Toggle MkToggle(RectTransform row, string label, Font font, bool initial, out Text labelText)
        {
            var go = new GameObject("Toggle_" + label.GetHashCode(), typeof(RectTransform), typeof(Toggle));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(row, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            var brt = bg.GetComponent<RectTransform>();
            brt.SetParent(rt, false);
            brt.anchorMin = new Vector2(0f, 0.5f);
            brt.anchorMax = new Vector2(0f, 0.5f);
            brt.pivot = new Vector2(0f, 0.5f);
            brt.sizeDelta = new Vector2(18f, 18f);
            brt.anchoredPosition = new Vector2(10f, 0f);
            var bimg = bg.GetComponent<Image>();
            bimg.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            bimg.type = Image.Type.Simple;
            bimg.color = CheckboxBorder;

            var ck = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            var crt = ck.GetComponent<RectTransform>();
            crt.SetParent(brt, false);
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(3f, 3f);
            crt.offsetMax = new Vector2(-3f, -3f);
            var cimg = ck.GetComponent<Image>();
            cimg.sprite = TacticalSpriteFactory.GetUiQuadSprite();
            cimg.type = Image.Type.Simple;
            cimg.color = CheckboxFill;

            var tg = go.GetComponent<Toggle>();
            tg.targetGraphic = bimg;
            tg.graphic = cimg;
            tg.isOn = initial;

            var txtGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var trt = txtGo.GetComponent<RectTransform>();
            trt.SetParent(rt, false);
            trt.anchorMin = new Vector2(0f, 0f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = new Vector2(34f, 0f);
            trt.offsetMax = new Vector2(-10f, 0f);
            labelText = txtGo.GetComponent<Text>();
            labelText.font = font;
            labelText.fontSize = 13;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = new Color(0.93f, 0.94f, 0.96f, 1f);
            labelText.text = label;

            return tg;
        }
    }
}
