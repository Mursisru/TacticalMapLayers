using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TacticalMapLayers
{
    /// <summary>Single continuous thick polyline in UI local space (miter-style ribbon from averaged tangents).</summary>
    internal sealed class TacticalPolylineGraphic : MaskableGraphic
    {
        private readonly List<Vector2> _pts = new List<Vector2>(96);
        private float _halfWidth = 1f;
        private Color _lineColor = Color.white;

        public void SetLine(IReadOnlyList<Vector2> points, float halfWidthPixels, Color color)
        {
            _pts.Clear();
            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                    _pts.Add(points[i]);
            }

            _halfWidth = halfWidthPixels;
            _lineColor = color;
            SetVerticesDirty();
        }

        public void ClearLine()
        {
            _pts.Clear();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            int n = _pts.Count;
            if (n < 2 || _halfWidth <= 0.01f)
                return;

            var c = (Color32)_lineColor;
            float hw = _halfWidth;

            var left = new Vector2[n];
            var right = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                Vector2 tangent;
                if (i == 0)
                    tangent = (_pts[1] - _pts[0]).normalized;
                else if (i == n - 1)
                    tangent = (_pts[n - 1] - _pts[n - 2]).normalized;
                else
                {
                    tangent = _pts[i + 1] - _pts[i - 1];
                    if (tangent.sqrMagnitude < 1e-10f)
                        tangent = _pts[i + 1] - _pts[i];
                    tangent.Normalize();
                }

                Vector2 normal = new Vector2(-tangent.y, tangent.x) * hw;
                if (i > 0 && i < n - 1)
                {
                    Vector2 t0 = (_pts[i] - _pts[i - 1]).normalized;
                    Vector2 t1 = (_pts[i + 1] - _pts[i]).normalized;
                    float dot = Vector2.Dot(t0, t1);
                    if (dot < 0.55f)
                    {
                        Vector2 n0 = new Vector2(-t0.y, t0.x);
                        Vector2 n1 = new Vector2(-t1.y, t1.x);
                        Vector2 blended = (n0 + n1).normalized * hw;
                        float maxExtrude = hw * 4.2f;
                        if (blended.sqrMagnitude > maxExtrude * maxExtrude)
                            blended = blended.normalized * maxExtrude;
                        normal = blended;
                    }
                }

                left[i] = _pts[i] + normal;
                right[i] = _pts[i] - normal;
            }

            for (int i = 0; i < n - 1; i++)
            {
                int s = vh.currentVertCount;
                vh.AddVert(V(left[i], c));
                vh.AddVert(V(right[i], c));
                vh.AddVert(V(right[i + 1], c));
                vh.AddVert(V(left[i + 1], c));
                vh.AddTriangle(s, s + 1, s + 2);
                vh.AddTriangle(s, s + 2, s + 3);
            }
        }

        private static UIVertex V(Vector2 p, Color32 c)
        {
            return new UIVertex
            {
                position = new Vector3(p.x, p.y, 0f),
                color = c,
                uv0 = Vector2.zero
            };
        }
    }
}
