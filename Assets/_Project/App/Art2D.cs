using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>Helpers para construir objetos 2D (sprites y unidades animadas).</summary>
    public static class Art2D
    {
        private static Sprite _solid;

        /// <summary>Sprite blanco 1x1 (para fondos/recuadros tintados con SpriteRenderer.color).</summary>
        public static Sprite SolidSprite()
        {
            if (_solid != null) return _solid;
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _solid = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _solid;
        }

        public static GameObject SpriteObject(Transform parent, string name, Sprite sprite,
            Vector3 pos, int order, float scale = 1f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return go;
        }
    }
}
