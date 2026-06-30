using UnityEngine;

namespace TacticalMapLayers
{
    internal static class TacticalSpriteFactory
    {
        private static Sprite _circle;
        private static Sprite _uiQuad;

        private const int TextureSize = 2048;

        /// <summary>Opaque white quad for UI Images (checkboxes, flat fills). Unity Images without a sprite can render incorrectly.</summary>
        public static Sprite GetUiQuadSprite()
        {
            if (_uiQuad != null)
                return _uiQuad;

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "TacticalMapLayers_UiQuad"
            };
            var block = new Color32[16];
            for (int i = 0; i < 16; i++)
                block[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(block);
            tex.Apply(false, true);
            _uiQuad = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            _uiQuad.name = "TacticalUiQuadSprite";
            return _uiQuad;
        }

        /// <summary>White circle, pivot center; rect size sets diameter on map.</summary>
        public static Sprite GetWhiteCircle()
        {
            if (_circle != null)
                return _circle;

            int size = TextureSize;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "TacticalMapLayers_Circle"
            };
            var pixels = new Color32[size * size];
            float r = size * 0.48f;
            float edge = 4f;
            float cx = size * 0.5f;
            float cy = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    byte a;
                    if (d <= r - edge)
                        a = 255;
                    else if (d >= r)
                        a = 0;
                    else
                        a = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01((r - d) / edge));
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            _circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _circle.name = "TacticalCircleSprite";
            return _circle;
        }
    }
}
