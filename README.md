# Tactical Map Layers

BepInEx plugin for **[Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/)** that adds a **centered layers panel** on the **fullscreen (maximized) tactical map**. Optional overlays: radar coverage disks, allied radars, enemy bases/airfields, and an approximate **front line**. The mod does **not** change game mechanics or networking; it reads public game state and draws UI under the map’s icon layer.

**Plugin GUID:** `com.at747.tacticalmaplayers` · **Version:** see `TacticalMapLayersPlugin.PluginVersion` (matches `CHANGELOG.md`).

## Requirements

- Nuclear Option (Windows), **BepInEx 5** x64.

## Features

- **Hotkey** (default **F10**) toggles the panel while the map is **maximized** only.
- **Tabs:** Tactical · Front · Settings — labels follow **Interface → Language** (`ru` / `en`).
- **Tactical:** radar and facility layers (see table below).
- **Front:** front-line overlay + optional hint text.
- **Settings:** language, front-line hint toggle, “turn off all map overlays”.
- Layer visibility and UI options persist in BepInEx **`BepInEx/config/com.at747.tacticalmaplayers.cfg`** (including last tab: **`Interface.LayersPanelMainTab`**).

## Build

1. Clone or copy this repository folder (`TacticalMapLayers` at repo root).
2. Copy `Directory.Build.user.props.example` → `Directory.Build.user.props` and set **`NuclearOptionRoot`** if your game is not at the default Steam path (see `Directory.Build.props`).
3. Open **`TacticalMapLayers.sln`** (or **`TacticalMapLayers.slnx`**) in Visual Studio, configuration **Release**, build the **TacticalMapLayers** project — or from a Developer Prompt:

```text
msbuild TacticalMapLayers\TacticalMapLayers.csproj /p:Configuration=Release
```

4. Copy **`TacticalMapLayers\bin\Release\TacticalMapLayers.dll`** to **`Nuclear Option\BepInEx\plugins\`**.

> This folder is the **canonical** layout for GitHub (`TacticalMapLayers\TacticalMapLayers.csproj`). If you maintain a separate local VS worktree elsewhere, keep its sources in sync with this tree; the built artifact is always **`TacticalMapLayers.dll`**.

## Usage

1. Open the tactical map and **maximize** it (fullscreen map).
2. Press the configured hotkey (**F10** by default) to show or hide the **layers panel**.
3. Use the **Tactical** / **Front** / **Settings** tabs and toggles as needed.

### Layers (Tactical tab)

| Config key (Radar / Other / Analysis) | Meaning |
|----------------------------------------|---------|
| **EnemyGroundRadars** | Enemy radars on **land vehicles, buildings, and ships** (not aircraft). Filled disk ≈ detection range (360° simplification). |
| **AllyStationaryRadars** | Allied **stationary** radars. |
| **AllyAllRadars** | **All** allied radars (360° disks). |
| **EnemyAirbases** | Enemy-held **bases / airfields** (approximate highlight). |
| **FrontLine** (Front tab) | Approximate **front line** between friendly and **visible** enemy ground/naval forces. |

## Configuration (BepInEx)

| Section | Notes |
|---------|--------|
| **Interface** | `Language` (`ru` / `en`), `ShowFrontLineHint`, **`LayersPanelMainTab`** (0 Tactical, 1 Front, 2 Settings). |
| **Radar layers** | `EnemyGroundRadars`, `AllyStationaryRadars`, `AllyAllRadars`. |
| **Other layers** | `EnemyAirbases`. |
| **Analysis layers** | `FrontLine`. |
| **Hotkeys** | `ToggleLayerPanel` (default F10). |

## Limitations

- **Radar azimuth** (narrow cones) is not rendered faithfully; disks use **360°** at the radar’s max range.
- **Airbase** highlights use a fixed world radius for visibility, not the exact capture footprint.
- Overlays refresh at an adaptive interval while the map is maximized; very large numbers of radars may cost some CPU.

## Manual test checklist

1. Maximize map → **F10** opens/closes the panel; tabs **Tactical / Front / Settings** switch correctly in both languages.
2. Enable **EnemyGroundRadars** — disks appear; minimize map → overlays hidden; maximize again → **F10** still works after closing the map with the panel open.
3. **Front** tab: **FrontLine** on/off; hint visibility follows **ShowFrontLineHint**.
4. **Settings:** language chips, “turn off all map overlays”, config file updates **`LayersPanelMainTab`** when changing tabs.
5. No repeated errors in `BepInEx/LogOutput.log` during open/close cycles.

## License

[MIT](LICENSE)
