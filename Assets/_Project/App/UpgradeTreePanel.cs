using System;
using System.Collections.Generic;
using AutoBattle.Meta.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    public class UpgradeTreePanel
    {
        public readonly GameObject Root;

        private readonly GameContext _ctx;
        private readonly Action _onChanged;
        private readonly ArtConfig _art;
        private readonly Text _info;
        private readonly Text _status;
        private readonly RectTransform _treeContent;
        private readonly List<NodeView> _nodeViews = new();
        private readonly List<GameObject> _lines = new();

        private static readonly Color TintLocked = new(0.4f, 0.4f, 0.4f);
        private static readonly Color TintAvailable = Color.white;
        private static readonly Color TintOwned = new(0.5f, 1f, 0.6f);

        private static readonly string[] Icons = { "⚔", "🛡", "❤", "⚡", "✦", "☄", "◆", "★", "⬧", "⊕" };

        public UpgradeTreePanel(Transform canvas, GameContext ctx, ArtConfig art, Action onChanged)
        {
            _ctx = ctx;
            _art = art;
            _onChanged = onChanged;

            // Opaque base so nothing behind is visible
            Root = UIFactory.Panel(canvas, "UpgradeTreePanel", new Color(0.08f, 0.06f, 0.04f, 1f));
            UIFactory.Stretch(Root.GetComponent<RectTransform>());

            // Wood table on top, stretched edge-to-edge
            if (art != null && art.woodTable != null)
            {
                var tableGO = new GameObject("WoodBG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                tableGO.transform.SetParent(Root.transform, false);
                UIFactory.Stretch(tableGO.GetComponent<RectTransform>());
                var tableImg = tableGO.GetComponent<Image>();
                tableImg.sprite = art.woodTable;
                tableImg.type = Image.Type.Simple;
                tableImg.preserveAspect = false;
                tableImg.raycastTarget = false;
            }

            var title = UIFactory.Label(Root.transform, "UPGRADES TREE", 36, TextAnchor.UpperLeft, Color.white);
            UIFactory.Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(400, 50), new Vector2(20, -15));

            _info = UIFactory.Label(Root.transform, "", 26, TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.3f));
            UIFactory.Anchor(_info.rectTransform, new Vector2(0, 1), new Vector2(300, 40), new Vector2(20, -65));

            _status = UIFactory.Label(Root.transform, "", 22, TextAnchor.LowerLeft, new Color(1, 0.9f, 0.6f));
            UIFactory.Anchor(_status.rectTransform, new Vector2(0, 0), new Vector2(900, 35), new Vector2(20, 15));

            // "VOLVER" button with ribbon style (same as BASE/MAPA)
            if (art != null && art.ribbonButton != null)
            {
                var closeBtn = UIFactory.ImageButton(Root.transform, art.ribbonButton, new Vector2(260, 100), Hide);
                UIFactory.Anchor(closeBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(260, 100), new Vector2(-15, 15));
                var closeTxt = UIFactory.Label(closeBtn.transform, "VOLVER", 30, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Stretch(closeTxt.rectTransform);
            }
            else
            {
                var close = UIFactory.Button(Root.transform, "VOLVER", new Color(0.4f, 0.4f, 0.45f), Hide);
                UIFactory.Anchor(close.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(180, 60), new Vector2(-15, 15));
            }

            // Viewport: drag to pan, scroll wheel to zoom
            var viewport = UIFactory.Panel(Root.transform, "Viewport", new Color(0, 0, 0, 0));
            var vpRT = viewport.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = new Vector2(10, 60);
            vpRT.offsetMax = new Vector2(-10, -100);
            viewport.AddComponent<RectMask2D>();

            var zoomPan = viewport.AddComponent<TreeZoomPan>();

            var content = new GameObject("TreeContent", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            _treeContent = content.GetComponent<RectTransform>();
            _treeContent.anchorMin = _treeContent.anchorMax = _treeContent.pivot = new Vector2(0.5f, 0.5f);
            _treeContent.sizeDelta = new Vector2(3000, 2000);
            _treeContent.anchoredPosition = Vector2.zero;

            zoomPan.content = _treeContent;

            Root.SetActive(false);
        }

        public void Show()
        {
            Root.SetActive(true);
            _status.text = "";
            Rebuild();
        }

        public void Hide() => Root.SetActive(false);

        private void Rebuild()
        {
            foreach (var nv in _nodeViews) UnityEngine.Object.Destroy(nv.go);
            foreach (var l in _lines) UnityEngine.Object.Destroy(l);
            _nodeViews.Clear();
            _lines.Clear();

            _info.text = $"⊙ {_ctx.State.wallet.coins}";

            var tree = _ctx.Config.upgradeTree;
            if (tree == null || tree.nodes == null || tree.nodes.Length == 0)
            {
                _status.text = "No hay árbol configurado.";
                return;
            }

            var posMap = new Dictionary<string, Vector2>();
            foreach (var node in tree.nodes)
            {
                if (node == null) continue;
                posMap[node.id] = node.treePosition;
            }

            // Lines first (behind nodes)
            foreach (var node in tree.nodes)
            {
                if (node == null || node.prerequisiteIds == null) continue;
                if (!posMap.ContainsKey(node.id)) continue;
                if (!IsVisible(node)) continue;

                foreach (var preId in node.prerequisiteIds)
                {
                    if (string.IsNullOrEmpty(preId) || !posMap.ContainsKey(preId)) continue;
                    var preNode = tree.Get(preId);
                    if (preNode != null && !IsVisible(preNode)) continue;

                    bool active = _ctx.State.upgrades.IsUnlocked(preId);
                    _lines.Add(CreateLine(posMap[preId], posMap[node.id], active));
                }
            }

            // Nodes
            foreach (var node in tree.nodes)
            {
                if (node == null || !IsVisible(node)) continue;

                bool owned = _ctx.State.upgrades.IsUnlocked(node.id);
                var can = UpgradeService.CanPurchase(_ctx.State, node);
                _nodeViews.Add(CreateNodeView(node, posMap[node.id], owned, can.success));
            }
        }

        private bool IsVisible(UpgradeNode node)
        {
            if (_ctx.State.upgrades.IsUnlocked(node.id)) return true;
            if (node.prerequisiteIds == null || node.prerequisiteIds.Length == 0) return true;
            foreach (var preId in node.prerequisiteIds)
            {
                if (string.IsNullOrEmpty(preId)) continue;
                if (_ctx.State.upgrades.IsUnlocked(preId)) return true;
            }
            return false;
        }

        private NodeView CreateNodeView(UpgradeNode node, Vector2 pos, bool owned, bool canBuy)
        {
            float nodeSize = 88f;

            // Single GO with the button sprite — no border
            var nodeGO = new GameObject($"Node_{node.id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            nodeGO.transform.SetParent(_treeContent, false);
            var rt = nodeGO.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(nodeSize, nodeSize);
            rt.anchoredPosition = pos;

            var img = nodeGO.GetComponent<Image>();
            if (_art != null && _art.skillNodeButton != null)
            {
                img.sprite = _art.skillNodeButton;
                img.type = Image.Type.Simple;
                img.color = owned ? TintOwned : (canBuy ? TintAvailable : TintLocked);
            }
            else
            {
                img.color = owned ? new Color(0.15f, 0.55f, 0.25f)
                    : (canBuy ? new Color(0.2f, 0.45f, 0.65f)
                    : new Color(0.18f, 0.18f, 0.22f));
            }

            // Icon
            int idx = Mathf.Abs(node.id.GetHashCode()) % Icons.Length;
            var icon = UIFactory.Label(nodeGO.transform, Icons[idx], 34, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(icon.rectTransform);

            // Button
            var btn = nodeGO.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1, 1, 1, 0.85f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;
            var cap = node;
            btn.onClick.AddListener(() => OnNode(cap));

            // Label below
            string costText = node.cost > 0 ? $"\n{node.cost}g" : "\nGRATIS";
            var label = UIFactory.Label(nodeGO.transform, $"{node.displayName}{costText}",
                14, TextAnchor.UpperCenter, new Color(1, 1, 1, 0.9f));
            var lrt = label.rectTransform;
            lrt.anchorMin = lrt.anchorMax = lrt.pivot = new Vector2(0.5f, 1f);
            lrt.sizeDelta = new Vector2(160, 40);
            lrt.anchoredPosition = new Vector2(0, -nodeSize / 2f - 4f);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;

            return new NodeView { go = nodeGO, node = node };
        }

        private GameObject CreateLine(Vector2 from, Vector2 to, bool active)
        {
            var go = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_treeContent, false);
            go.transform.SetAsFirstSibling();

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

            var dir = to - from;
            float length = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rt.sizeDelta = new Vector2(length, active ? 3f : 2f);
            rt.anchoredPosition = (from + to) / 2f;
            rt.localRotation = Quaternion.Euler(0, 0, angle);

            go.GetComponent<Image>().color = active
                ? new Color(0.95f, 0.9f, 0.7f, 0.9f)
                : new Color(0.4f, 0.35f, 0.25f, 0.5f);

            return go;
        }

        private void OnNode(UpgradeNode node)
        {
            if (_ctx.State.upgrades.IsUnlocked(node.id))
            {
                _status.text = $"'{node.displayName}' ya comprado.";
                return;
            }

            var result = UpgradeService.Purchase(_ctx.State, node);
            if (result.success)
            {
                _ctx.Save();
                _status.text = $"Comprado: {node.displayName}";
                _onChanged?.Invoke();
            }
            else
            {
                _status.text = result.reason switch
                {
                    UpgradeFailReason.SinMonedas => "No tienes monedas suficientes.",
                    UpgradeFailReason.NivelBaseInsuficiente => $"Requiere nivel de base {node.requiredBaseLevel}.",
                    UpgradeFailReason.PrerequisitoFaltante => "Desbloquea el nodo anterior primero.",
                    UpgradeFailReason.YaDesbloqueado => "Ya comprado.",
                    _ => "No se puede comprar.",
                };
            }
            Rebuild();
        }

        private struct NodeView { public GameObject go; public UpgradeNode node; }
    }
}
