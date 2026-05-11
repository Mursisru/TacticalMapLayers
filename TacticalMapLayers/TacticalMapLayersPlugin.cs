using BepInEx;
using UnityEngine;

namespace TacticalMapLayers
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class TacticalMapLayersPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.at747.tacticalmaplayers";
        public const string PluginName = "Tactical Map Layers";
        public const string PluginVersion = "1.0.1";

        private void Awake()
        {
            var host = new GameObject("TacticalMapLayers_Host");
            Object.DontDestroyOnLoad(host);
            host.hideFlags = HideFlags.HideAndDontSave;
            host.AddComponent<TacticalMapLayersController>().Initialize(this, Logger);
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }
    }
}
