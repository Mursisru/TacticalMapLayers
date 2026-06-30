**Developer:** Mursisru

# Tactical Map Layers — Visual Studio engine workspace

[![Nuclear Option](https://img.shields.io/badge/Game-Nuclear%20Option-blue)](https://store.steampowered.com/app/2168680/Nuclear_Option/) [![BepInEx 5](https://img.shields.io/badge/Loader-BepInEx%205-orange)](https://docs.bepinex.dev/) [![Version](https://img.shields.io/badge/Version-0.0.0-green)]() [![License](https://img.shields.io/badge/License-MIT-lightgrey)](LICENSE)


This folder **`TacticalMapLayers_Engine`** under `source\repos\` is the **Visual Studio solution** for the same BepInEx mod as in **`Desktop\GITHUB local\TacticalMapLayers\`** (the **canonical** tree for GitHub). Edit and build here; keep sources aligned when you publish a release.

**Plugin GUID:** `com.at747.tacticalmaplayers`  
**Output DLL:** **`TacticalMapLayers.dll`** (same assembly name as the public repo; drop into `BepInEx\plugins\`).

## Open in Visual Studio

1. Open **`TacticalMapLayers_Engine.sln`** in this directory.
2. If the game is not at the default Steam path, copy **`Directory.Build.user.props.example`** → **`Directory.Build.user.props`** and set **`NuclearOptionRoot`**.
3. Build **Release**. Output: **`TacticalMapLayers_Engine\bin\Release\TacticalMapLayers.dll`**.

Command line:

```text
msbuild TacticalMapLayers_Engine\TacticalMapLayers_Engine.csproj /p:Configuration=Release
```

## Behaviour (summary)

BepInEx plugin for **[Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/)**: on the **maximized tactical map**, a **hotkey-toggled layers panel** (default **F10**) with **Tactical / Front / Settings** tabs — radar and facility overlays, optional **front line**, RU/EN UI, BepInEx persistence. Read-only client overlays; no gameplay or network changes.

Full feature list, config table, limitations, manual test checklist, and **first-time GitHub push** instructions: see **`README.md`** in **`Desktop\GITHUB local\TacticalMapLayers\`** (keep that file the single detailed doc).

## License

[MIT](LICENSE)

---

## Keywords

nuclear-option, bepinex, harmony, mod, tacticalmaplayers, csharp, unity
