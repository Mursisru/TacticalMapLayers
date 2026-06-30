# Changelog

## [0.0.0] - 2026-06-30

### Changed
- Documentation refresh: Developer header, badges, GitHub Alerts, Keywords, gitignore hygiene.


## 1.0.1

- **Fix:** Layers panel could stop opening (hotkey dead) after closing the map while the panel was open — `BuildOrMove` now runs while the map stays maximized so UI is recreated if the game destroys the map canvas under the same `DynamicMap` instance.
- **Interface:** Last panel tab persisted in config (`Interface.LayersPanelMainTab`, values 0–2).
- **UI:** Flat tactical panel (pre–“Win11” look), gold accent bar, checkbox controls use a proper quad sprite; RU/EN strings unchanged in spirit.

## 1.0.0

- Initial release: tactical overlays on the maximized map, BepInEx config for layers, layers panel (Tactical / Front / Settings), front-line polyline; client-side visualization only.
- **TacticalMapLayers_Engine** Visual Studio solution under `source\repos\` (same sources, **`TacticalMapLayers.dll`** output).
