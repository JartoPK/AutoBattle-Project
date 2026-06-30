using System;
using AutoBattle.Meta.Campaign;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoBattle.App
{
    public class MapWorld
    {
        public readonly GameObject Root;

        private static readonly Vector3 CamPos = new(0f, 0f, -10f);

        private readonly Action<CampaignNodeData> _onNodeClicked;

        public MapWorld(Transform parent, Action<CampaignNodeData> onNodeClicked)
        {
            _onNodeClicked = onNodeClicked;
            Root = new GameObject("MapWorld");
            Root.transform.SetParent(parent, false);
        }

        public void OnSceneLoaded()
        {
            var scene = SceneManager.GetSceneByName("Map");
            if (!scene.isLoaded) return;

            foreach (var go in scene.GetRootGameObjects())
            {
                var cam = go.GetComponent<Camera>();
                if (cam != null) cam.enabled = false;

                WireNodes(go.transform);
            }
        }

        public void Show(Camera cam)
        {
            Root.SetActive(true);
            cam.orthographic = true;
            cam.orthographicSize = 12f;
            cam.transform.SetPositionAndRotation(CamPos, Quaternion.identity);
        }

        public void Hide() => Root.SetActive(false);

        private void WireNodes(Transform root)
        {
            var mapNode = root.GetComponent<MapNode>();
            if (mapNode != null && mapNode.nodeData != null)
            {
                if (root.GetComponent<BoxCollider2D>() == null)
                {
                    var col = root.gameObject.AddComponent<BoxCollider2D>();
                    var sr = root.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                        col.size = sr.sprite.bounds.size;
                    else
                        col.size = new Vector2(1.5f, 1.5f);
                }

                var data = mapNode.nodeData;
                if (root.GetComponent<ClickableBuilding>() == null)
                    root.gameObject.AddComponent<ClickableBuilding>().OnClicked = () => _onNodeClicked?.Invoke(data);
            }

            foreach (Transform child in root)
                WireNodes(child);
        }
    }
}
