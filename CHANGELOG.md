# Changelog

## 1.0.0

- **Fix:** Layers panel could stop opening (hotkey dead) after closing the map while the panel was open — `BuildOrMove` now runs while the map stays maximized so UI is recreated if the game destroys the map canvas under the same `DynamicMap` instance.
- **Interface:** Last panel tab persisted in config (`Interface.LayersPanelMainTab`, values 0–2).
- **UI:** Flat tactical panel (pre–“Win11” look), gold accent bar, checkbox controls use a proper quad sprite; RU/EN strings unchanged in spirit.

## 0.9.9

- Initial public shape: tactical overlays on the maximized map, BepInEx config for layers, centered layers panel with Tactical / Front / Settings, front-line polyline, no Harmony.
