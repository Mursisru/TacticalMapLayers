using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TacticalMapLayers
{
    /// <summary>Draws radar disks and highlights under map icons (client-only visualization).</summary>
    public sealed class TacticalOverlayRenderer
    {
        private RectTransform _underlayRoot;
        private readonly List<Image> _pool = new List<Image>();
        private TacticalPolylineGraphic _frontLineGraphic;
        private readonly List<Vector2> _frontPolylineUi = new List<Vector2>(96);
        private float[] _frontDepthRaw;
        private float[] _frontDepthSmooth;

        /// <summary>Disk fill: 90% transparent (alpha 0.1).</summary>
        private const float RadarFillAlpha = 0.1f;

        private static readonly Color EnemyRadarFill = new Color(1f, 0.1f, 0.1f, RadarFillAlpha);
        private static readonly Color AllyRadarFill = new Color(0.2f, 0.55f, 1f, RadarFillAlpha);
        private static readonly Color AirbaseHighlight = new Color(1f, 0.2f, 0.85f, 0.7f);
        private static readonly Color FrontLineColor = new Color(1f, 0.92f, 0.15f, 0.88f);

        private const int FrontLineSliceCount = 64;
        private const float FrontLineThicknessPx = 2.5f;

        public TacticalOverlayRenderer()
        {
        }

        public void EnsureRoot(DynamicMap map)
        {
            if (_underlayRoot != null)
                return;

            var go = new GameObject("TacticalMapLayers_Underlay");
            go.layer = map.iconLayer.layer;
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(map.iconLayer.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.SetAsFirstSibling();
            _underlayRoot = rt;

            var flGo = new GameObject("TacticalFrontLineStrip", typeof(RectTransform), typeof(CanvasRenderer), typeof(TacticalPolylineGraphic));
            var flRt = flGo.GetComponent<RectTransform>();
            flRt.SetParent(_underlayRoot, false);
            flRt.anchorMin = Vector2.zero;
            flRt.anchorMax = Vector2.one;
            flRt.offsetMin = Vector2.zero;
            flRt.offsetMax = Vector2.zero;
            flRt.localScale = Vector3.one;
            _frontLineGraphic = flGo.GetComponent<TacticalPolylineGraphic>();
            _frontLineGraphic.raycastTarget = false;
        }

        public void DestroyRoot()
        {
            if (_underlayRoot == null)
                return;
            Object.Destroy(_underlayRoot.gameObject);
            _underlayRoot = null;
            _frontLineGraphic = null;
            _pool.Clear();
        }

        public void Refresh(DynamicMap map, TacticalLayersUi ui)
        {
            if (map == null || ui == null || _underlayRoot == null || !DynamicMap.mapMaximized)
            {
                HideAll();
                return;
            }

            if (!GameManager.GetLocalHQ(out var localHq))
            {
                HideAll();
                return;
            }

            float f = map.mapDisplayFactor;
            int idx = 0;

            if (ui.DrawEnemyGroundRadars || ui.DrawAllyStationaryRadars || ui.DrawAllyAllRadars)
            {
                var radars = Object.FindObjectsOfType<Radar>();
                foreach (var radar in radars)
                {
                    if (radar == null || !radar.gameObject.activeInHierarchy)
                        continue;
                    if (!radar.IsOperational())
                        continue;
                    var range = radar.GetRadarRange();
                    if (range < 1f)
                        continue;
                    var unit = radar.GetAttachedUnit();
                    if (unit == null || unit.disabled)
                        continue;
                    var hq = unit.NetworkHQ;
                    if (hq == null)
                        continue;
                    if (!IsVisibleToLocalHq(localHq, unit))
                        continue;

                    bool enemy = hq != localHq;
                    bool stationary = IsStationaryRadarHost(unit);
                    bool landOrShipRadar = IsNonAircraftRadarHost(unit);
                    bool showAllyAll = !enemy && ui.DrawAllyAllRadars;
                    bool showAllyStat = !enemy && ui.DrawAllyStationaryRadars && stationary;
                    if (enemy)
                    {
                        if (!ui.DrawEnemyGroundRadars || !landOrShipRadar)
                            continue;

                        var gp = radar.GetScanPoint().GlobalPosition();
                        DrawDisk(ref idx, f, gp, range * 2f, EnemyRadarFill);
                    }
                    else
                    {
                        if (!showAllyAll && !showAllyStat)
                            continue;
                        var gp = radar.GetScanPoint().GlobalPosition();
                        if (showAllyAll)
                            DrawDisk(ref idx, f, gp, range * 2f, AllyRadarFill);
                        if (showAllyStat && stationary)
                            DrawDisk(ref idx, f, gp, range * 2f, AllyRadarFill);
                    }
                }
            }

            if (ui.DrawEnemyAirbases)
            {
                foreach (var ab in Object.FindObjectsOfType<Airbase>())
                {
                    if (ab == null || ab.disabled)
                        continue;
                    if (ab.CurrentHQ == null || ab.CurrentHQ == localHq)
                        continue;
                    if (ab.center == null)
                        continue;
                    if (!IsAirbaseVisible(localHq, ab))
                        continue;

                    var gp = ab.center.GlobalPosition();
                    float approx = 900f;
                    DrawDisk(ref idx, f, gp, approx, AirbaseHighlight);
                }
            }

            if (ui.DrawFrontLine)
                DrawFrontLine(ref idx, map, f, localHq);
            else
                _frontLineGraphic?.ClearLine();

            for (int i = idx; i < _pool.Count; i++)
            {
                if (_pool[i] != null)
                    _pool[i].enabled = false;
            }

            if (ui.DrawFrontLine && _frontLineGraphic != null)
                _frontLineGraphic.rectTransform.SetAsLastSibling();
        }

        private void DrawFrontLine(ref int idx, DynamicMap map, float mapFactor, FactionHQ localHq)
        {
            if (_frontLineGraphic == null)
                return;

            var friendly = new List<Vector2>(64);
            var enemy = new List<Vector2>(64);
            foreach (var unit in UnitRegistry.allUnits)
            {
                if (unit == null || unit.disabled)
                    continue;
                if (!(unit is GroundVehicle) && !(unit is Ship))
                    continue;
                var hq = unit.NetworkHQ;
                if (hq == null)
                    continue;
                if (!IsVisibleToLocalHq(localHq, unit))
                    continue;
                var p = unit.GlobalPosition();
                var xz = new Vector2(p.x, p.z);
                if (hq == localHq)
                    friendly.Add(xz);
                else
                    enemy.Add(xz);
            }

            if (friendly.Count == 0 || enemy.Count == 0)
            {
                _frontLineGraphic.ClearLine();
                return;
            }

            Vector2 cf = AverageXZ(friendly);
            Vector2 ce = AverageXZ(enemy);
            Vector2 delta = ce - cf;
            if (delta.sqrMagnitude < 400f * 400f)
            {
                _frontLineGraphic.ClearLine();
                return;
            }

            Vector2 mid = (cf + ce) * 0.5f;
            Vector2 dir = delta.normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);
            float extentWorld = map.mapDimension * 0.55f;
            float sigma = Mathf.Clamp(extentWorld * 0.12f, 550f, 13000f);
            float sigmaSq = sigma * sigma;
            float invTwoSigmaSq = 1f / (2f * sigmaSq);

            int n = FrontLineSliceCount;
            EnsureFrontDepthBuffers(n + 1);

            float depthCf = Vector2.Dot(cf - mid, dir);
            float depthCe = Vector2.Dot(ce - mid, dir);
            float depthClampMin = Mathf.Min(depthCf, depthCe) - extentWorld * 0.4f;
            float depthClampMax = Mathf.Max(depthCf, depthCe) + extentWorld * 0.4f;

            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n;
                float u = (t - 0.5f) * 2f * extentWorld;

                float sumWf = 0f, sumWdF = 0f;
                for (int fi = 0; fi < friendly.Count; fi++)
                {
                    Vector2 p = friendly[fi];
                    float lateral = Vector2.Dot(p - mid, perp);
                    float dAlong = Vector2.Dot(p - mid, dir);
                    float dl = lateral - u;
                    float w = Mathf.Exp(-(dl * dl) * invTwoSigmaSq);
                    sumWf += w;
                    sumWdF += w * dAlong;
                }

                float sumWe = 0f, sumWdE = 0f;
                for (int ei = 0; ei < enemy.Count; ei++)
                {
                    Vector2 p = enemy[ei];
                    float lateral = Vector2.Dot(p - mid, perp);
                    float dAlong = Vector2.Dot(p - mid, dir);
                    float dl = lateral - u;
                    float w = Mathf.Exp(-(dl * dl) * invTwoSigmaSq);
                    sumWe += w;
                    sumWdE += w * dAlong;
                }

                float depthF = sumWf > 0.02f ? sumWdF / sumWf : depthCf;
                float depthE = sumWe > 0.02f ? sumWdE / sumWe : depthCe;
                float depth = Mathf.Clamp((depthF + depthE) * 0.5f, depthClampMin, depthClampMax);
                _frontDepthRaw[i] = depth;
            }

            _frontDepthSmooth[0] = _frontDepthRaw[0];
            _frontDepthSmooth[n] = _frontDepthRaw[n];
            for (int i = 1; i < n; i++)
                _frontDepthSmooth[i] = 0.22f * _frontDepthRaw[i - 1] + 0.56f * _frontDepthRaw[i] + 0.22f * _frontDepthRaw[i + 1];

            _frontPolylineUi.Clear();
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n;
                float u = (t - 0.5f) * 2f * extentWorld;
                float depth = _frontDepthSmooth[i];
                Vector2 worldPt = mid + perp * u + dir * depth;
                _frontPolylineUi.Add(worldPt * mapFactor);
            }

            _frontLineGraphic.SetLine(_frontPolylineUi, FrontLineThicknessPx * 0.5f, FrontLineColor);
        }

        private void EnsureFrontDepthBuffers(int len)
        {
            if (_frontDepthRaw != null && _frontDepthRaw.Length >= len)
                return;
            _frontDepthRaw = new float[len];
            _frontDepthSmooth = new float[len];
        }

        private static Vector2 AverageXZ(List<Vector2> pts)
        {
            Vector2 s = Vector2.zero;
            for (int i = 0; i < pts.Count; i++)
                s += pts[i];
            return s / Mathf.Max(1, pts.Count);
        }

        private static bool IsStationaryRadarHost(Unit unit)
        {
            if (unit is Building)
                return true;
            if (unit is GroundVehicle gv)
                return gv.NetworknetworkStationary;
            return false;
        }

        /// <summary>Buildings, ground vehicles, and ships — excludes aircraft-borne radars only.</summary>
        private static bool IsNonAircraftRadarHost(Unit unit)
        {
            if (unit == null)
                return false;
            if (unit is Aircraft)
                return false;
            return unit is Building || unit is GroundVehicle || unit is Ship;
        }

        private static bool IsVisibleToLocalHq(FactionHQ localHq, Unit unit)
        {
            if (localHq == null || unit == null || unit.NetworkHQ == null)
                return false;
            if (unit.NetworkHQ == localHq)
                return true;
            UnitMapIcon icon;
            if (!DynamicMap.TryGetMapIcon(unit, out icon))
                return false;
            return icon != null && icon.gameObject.activeInHierarchy;
        }

        private static bool IsAirbaseVisible(FactionHQ localHq, Airbase airbase)
        {
            if (localHq == null || airbase == null || airbase.CurrentHQ == null)
                return false;

            Unit host;
            if (airbase.TryGetAttachedUnit(out host) && host != null)
                return IsVisibleToLocalHq(localHq, host);

            GlobalPosition center = airbase.center.GlobalPosition();
            const float revealRadius = 1200f;
            foreach (var unit in UnitRegistry.allUnits)
            {
                if (unit == null || unit.disabled)
                    continue;
                if (unit.NetworkHQ != airbase.CurrentHQ)
                    continue;
                if (!FastMath.InRange(unit.GlobalPosition(), center, revealRadius))
                    continue;
                if (IsVisibleToLocalHq(localHq, unit))
                    return true;
            }

            return false;
        }

        private void DrawDisk(ref int idx, float mapFactor, GlobalPosition gp, float diameterMeters, Color color)
        {
            var img = Rent(ref idx);
            var rt = img.rectTransform;
            rt.localRotation = Quaternion.identity;
            var pos = gp.AsVector3() * mapFactor;
            rt.localPosition = new Vector3(pos.x, pos.z, 0f);
            float d = diameterMeters * mapFactor;
            rt.sizeDelta = new Vector2(d, d);
            img.sprite = TacticalSpriteFactory.GetWhiteCircle();
            img.preserveAspect = true;
            img.color = color;
            img.enabled = true;
        }

        private Image Rent(ref int idx)
        {
            while (idx >= _pool.Count)
            {
                var go = new GameObject("RadarDisk", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.layer = _underlayRoot.gameObject.layer;
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(_underlayRoot, false);
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
                var img = go.GetComponent<Image>();
                img.sprite = TacticalSpriteFactory.GetWhiteCircle();
                img.type = Image.Type.Simple;
                img.raycastTarget = false;
                img.preserveAspect = true;
                _pool.Add(img);
            }

            return _pool[idx++];
        }

        private void HideAll()
        {
            _frontLineGraphic?.ClearLine();
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] != null)
                    _pool[i].enabled = false;
            }
        }
    }
}
