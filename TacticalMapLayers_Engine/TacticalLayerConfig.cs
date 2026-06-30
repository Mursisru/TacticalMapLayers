using BepInEx.Configuration;
using UnityEngine;

namespace TacticalMapLayers
{
    /// <summary>Default layer visibility (saved by BepInEx).</summary>
    public sealed class TacticalLayerConfig
    {
        public ConfigEntry<bool> LayerEnemyGroundRadars { get; }
        public ConfigEntry<bool> LayerAllyStationaryRadars { get; }
        public ConfigEntry<bool> LayerAllyAllRadars { get; }
        public ConfigEntry<bool> LayerEnemyAirbases { get; }
        public ConfigEntry<bool> LayerFrontLine { get; }
        public ConfigEntry<KeyboardShortcut> ToggleLayerPanelHotkey { get; }
        public ConfigEntry<string> UiLanguage { get; }
        public ConfigEntry<bool> ShowFrontLineHint { get; }
        /// <summary>0 = Tactical, 1 = Front, 2 = Settings (layers panel tab).</summary>
        public ConfigEntry<int> LayersPanelMainTab { get; }

        public TacticalLayerConfig(ConfigFile config)
        {
            const string radar = "Radar layers";
            LayerEnemyGroundRadars = config.Bind(radar, "EnemyGroundRadars", false, "Enemy radars on land, vehicles, and ships (not aircraft). Filled disk = detection range.");
            LayerAllyStationaryRadars = config.Bind(radar, "AllyStationaryRadars", false, "Blue disks: allied stationary radars.");
            LayerAllyAllRadars = config.Bind(radar, "AllyAllRadars", false, "Blue disks: all allied radars.");

            const string other = "Other layers";
            LayerEnemyAirbases = config.Bind(other, "EnemyAirbases", false, "Highlight enemy-controlled airbases / bases.");

            const string analysis = "Analysis layers";
            LayerFrontLine = config.Bind(analysis, "FrontLine", false, "Front tab: approximate front line between friendly and visible enemy ground/naval units.");

            const string hotkeys = "Hotkeys";
            ToggleLayerPanelHotkey = config.Bind(hotkeys, "ToggleLayerPanel", new KeyboardShortcut(KeyCode.F10),
                "While the tactical map is maximized: show or hide the layers panel (restores last tab from Interface.LayersPanelMainTab). Configure in BepInEx Configuration Manager. / Только при развёрнутой карте: показать или скрыть панель слоёв.");

            const string ui = "Interface";
            UiLanguage = config.Bind(ui, "Language", "ru",
                new ConfigDescription(
                    "Panel language: ru (Russian) or en (English).",
                    new AcceptableValueList<string>(new[] { "ru", "en" })));
            ShowFrontLineHint = config.Bind(ui, "ShowFrontLineHint", true,
                "Show the explanatory note on the Front tab.");
            LayersPanelMainTab = config.Bind(ui, "LayersPanelMainTab", 0,
                new ConfigDescription(
                    "Last-selected tab in the layers panel: 0 Tactical, 1 Front, 2 Settings.",
                    new AcceptableValueRange<int>(0, 2)));
        }
    }
}
