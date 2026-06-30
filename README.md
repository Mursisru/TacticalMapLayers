**Developer:** Mursisru

# Tactical Map Layers

[![Nuclear Option](https://img.shields.io/badge/Game-Nuclear%20Option-blue)](https://store.steampowered.com/app/2168680/Nuclear_Option/)
[![BepInEx 5](https://img.shields.io/badge/Loader-BepInEx%205-orange)](https://docs.bepinex.dev/)
[![Version](https://img.shields.io/badge/Version-1.0.1-green)](https://github.com/Mursisru/TacticalMapLayers/releases/tag/v1.0.1)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow)](https://github.com/Mursisru/TacticalMapLayers/blob/main/LICENSE)

BepInEx 5 plugin for **[Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/)**: on the **maximized tactical map**, a hotkey-toggled **layers panel** (default **F10**) with **Tactical / Front / Settings** tabs — radar and facility overlays, optional **front line**, RU/EN UI, BepInEx persistence. Read-only client overlays; **no gameplay or network changes**.

**Plugin GUID:** `com.at747.tacticalmaplayers`  
**Output DLL:** `TacticalMapLayers.dll` → `BepInEx\plugins\`

> [!NOTE]
> **Engine workspace:** active development solution lives in `source\repos\TacticalMapLayers_Engine\`. This repo is the GitHub mirror; keep both trees aligned via robocopy before release.

---

## Critical warnings

> [!IMPORTANT]
> **BepInEx 5 (x64) required** - install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) before this mod.

> [!WARNING]
> **Requires maximized tactical map** - panel does not appear on small MFD map modes.

> [!WARNING]
> **Front line is heuristic** - fog-of-war and intel limits apply; untested vs mods that replace tactical map UI root.

> [!NOTE]
> **Multiplayer safe** - read-only client overlays; no gameplay or network changes.

## Table of contents

- [Critical warnings](#critical-warnings)
- [Features](#features)
- [Requirements](#requirements)
- [Install](#install)
- [Controls](#controls)
- [Configuration](#configuration)
- [Limitations](#limitations)
- [Build](#build)
- [Manual test checklist](#manual-test-checklist)
- [License](#license)

## Features

| Tab | Overlays |
|-----|----------|
| **Tactical** | Enemy ground radars (land/vehicles/ships), allied stationary radars, allied all radars, enemy airbases |
| **Front** | Approximate front line between friendly and visible enemy ground/naval units |
| **Settings** | UI language (RU/EN), front-line hint toggle, default panel tab |

All layers are **observation-only** — they do not alter simulation, targeting, or network state.

---

## Requirements

- **[Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/)** (Steam)
- **[BepInEx 5](https://docs.bepinex.dev/)** x64
- **[Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager)** (recommended)

---

## Install

> [!IMPORTANT]
> **BepInEx 5 (x64) required** — install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) before this mod.

1. Download **`TacticalMapLayers.dll`** from [Releases](https://github.com/Mursisru/TacticalMapLayers/releases) or build Release from `TacticalMapLayers_Engine`.
2. Copy to:

   ```text
   Nuclear Option\BepInEx\plugins\TacticalMapLayers.dll
   ```

3. Config: `BepInEx\config\com.at747.tacticalmaplayers.cfg`

---

## Controls

| Input | Action |
|-------|--------|
| **F10** (default) | Toggle layers panel on maximized tactical map |

Rebind via Configuration Manager → `Hotkeys.ToggleLayerPanel`.

---

## Configuration

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| `Radar` | `EnemyGroundRadars` | `false` | Enemy radars on land, vehicles, ships (not aircraft) |
| `Radar` | `AllyStationaryRadars` | `false` | Allied stationary radar disks |
| `Radar` | `AllyAllRadars` | `false` | All allied radar disks |
| `Other` | `EnemyAirbases` | `false` | Highlight enemy-controlled airbases |
| `Analysis` | `FrontLine` | `false` | Front tab: approximate front line |
| `Hotkeys` | `ToggleLayerPanel` | `F10` | Open/close panel |
| `UI` | `Language` | `ru` | Panel language (`ru` / `en`) |
| `UI` | `ShowFrontLineHint` | `true` | Show front-line help text |
| `UI` | `LayersPanelMainTab` | `0` | Default tab index on open |

---

## Limitations

- Requires **maximized tactical map** — panel does not appear on small MFD map modes.
- Front line is **heuristic** from visible units; fog-of-war and intel limits apply.
- MP-safe (client overlay only) but untested against mods that replace the tactical map UI root.

---

## Build

**From engine workspace** (`source\repos\TacticalMapLayers_Engine`):

1. Open `TacticalMapLayers_Engine.sln`.
2. Set `NuclearOptionRoot` in `Directory.Build.user.props` if needed.
3. Build **Release**.

```powershell
msbuild TacticalMapLayers_Engine\TacticalMapLayers_Engine.csproj /p:Configuration=Release
```

Output: `TacticalMapLayers_Engine\bin\Release\TacticalMapLayers.dll`

---

## Manual test checklist

1. Enter mission → open **maximized tactical map**.
2. Press **F10** — layers panel appears with three tabs.
3. Enable **Enemy ground radars** — disks appear at expected sites.
4. Switch **Front** tab → enable front line — line draws between force clusters.
5. Change **UI language** to `en` — labels switch without restart.
6. Close map / pause — panel hides; no errors in `BepInEx\LogOutput.log`.

---

## License

MIT — see [LICENSE](LICENSE).

---

## Keywords

nuclear-option, bepinex, harmony, mod, tactical-map, radar-overlay, front-line, csharp, unity
