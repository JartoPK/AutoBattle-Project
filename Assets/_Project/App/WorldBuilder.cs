using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>Helpers para construir mundos 3D con primitivas (arte placeholder).</summary>
    public static class WorldBuilder
    {
        public static Material Lit(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader != null ? shader : Shader.Find("Standard"));
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            return mat;
        }

        public static GameObject Ground(Transform parent, string name, Vector3 center, float size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane); // el Plane mide 10x10
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.localScale = new Vector3(size / 10f, 1f, size / 10f);
            go.GetComponent<Renderer>().sharedMaterial = Lit(color);
            return go;
        }

        public static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Lit(color);
            return go;
        }

        public static GameObject Label3D(Transform parent, string text, Vector3 pos)
        {
            var go = new GameObject("Label3D");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(35f, 0f, 0f); // inclinado hacia la cámara

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = UIFactory.DefaultFont;
            tm.fontSize = 64;
            tm.characterSize = 0.1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            go.GetComponent<MeshRenderer>().sharedMaterial = UIFactory.DefaultFont.material;
            return go;
        }
    }
}
