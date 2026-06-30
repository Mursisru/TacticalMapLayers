using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace TacticalMapLayers
{
    public sealed class TacticalMapLayersController : MonoBehaviour
    {
        private TacticalLayerConfig _cfg;
        private TacticalLayersUi _ui;
        private TacticalOverlayRenderer _renderer;
        private DynamicMap _map;
        private bool _subscribed;
        private float _nextRadarScan;

        public void Initialize(BaseUnityPlugin plugin, ManualLogSource log)
        {
            _cfg = new TacticalLayerConfig(plugin.Config);
            _ui = new TacticalLayersUi(_cfg, plugin.Config, log);
            _renderer = new TacticalOverlayRenderer();
        }

        private void OnDestroy()
        {
            Unhook();
            _ui?.Destroy();
            _renderer?.DestroyRoot();
        }

        private void Update()
        {
            var map = SceneSingleton<DynamicMap>.i;
            if (map == null)
            {
                if (_map != null)
                    TeardownMap();
                return;
            }

            if (_map != map)
            {
                TeardownMap();
                _map = map;
                Hook(map);
            }

            if (!_subscribed)
                return;

            if (DynamicMap.mapMaximized)
            {
                _ui?.BuildOrMove(map);

                if (_cfg != null && _cfg.ToggleLayerPanelHotkey != null && _cfg.ToggleLayerPanelHotkey.Value.IsDown())
                    _ui?.OnHotkeyToggleLayersPanel();

                if (_ui != null && _renderer != null)
                {
                    _renderer.EnsureRoot(map);
                    float interval = (_ui.TacticalPanelVisible || _ui.AnyOverlayLayerEnabled) ? 0.08f : 0.4f;
                    if (Time.unscaledTime >= _nextRadarScan)
                    {
                        _nextRadarScan = Time.unscaledTime + interval;
                        _renderer.Refresh(map, _ui);
                    }
                }
            }
            else
            {
                _renderer?.Refresh(null, _ui);
            }
        }

        private void Hook(DynamicMap map)
        {
            map.onMapMaximized += OnMaximized;
            map.onMapMinimized += OnMinimized;
            _subscribed = true;
            _ui.BuildOrMove(map);
            _renderer.EnsureRoot(map);
        }

        private void Unhook()
        {
            if (!_subscribed)
                return;
            if (_map != null)
            {
                _map.onMapMaximized -= OnMaximized;
                _map.onMapMinimized -= OnMinimized;
            }
            _subscribed = false;
        }

        private void TeardownMap()
        {
            Unhook();
            _ui?.Destroy();
            _renderer?.DestroyRoot();
            _map = null;
        }

        private void OnMaximized()
        {
            _ui?.BuildOrMove(_map);
            _ui?.OnMapMaximizedChanged(true);
        }

        private void OnMinimized()
        {
            _ui?.OnMapMaximizedChanged(false);
            _renderer?.Refresh(null, _ui);
        }
    }
}
