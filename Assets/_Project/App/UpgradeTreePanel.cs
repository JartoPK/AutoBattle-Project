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
        private readonly Text _coinText;
        private readonly RectTransform _treeContent;
        private readonly List<NodeView> _nodeViews = new();
        private readonly List<GameObject> _lines = new();

        // Detail panel
        private readonly GameObject _detailPanel;
        private readonly Text _detailTitle;
        private readonly Text _detailDesc;
        private readonly Text _detailCost;
        private readonly Button _buyBtn;
        private readonly Text _buyLabel;
        private readonly Text _detailStatus;
        private UpgradeNode _selectedNode;

        private static readonly Color TintLocked = new(0.4f, 0.4f, 0.4f);
        private static readonly Color TintAvailable = Color.white;
        private static readonly Color TintOwned = new(0.5f, 1f, 0.6f);

        public UpgradeTreePanel(Transform canvas, GameContext ctx, ArtConfig art, Action onChanged)
        {
            _ctx = ctx;
            _art = art;
            _onChanged = onChanged;

            // ── Root (opaque) ──
            Root = UIFactory.Panel(canvas, "UpgradeTreePanel", new Color(0.08f, 0.06f, 0.04f, 1f));
            UIFactory.Stretch(Root.GetComponent<RectTransform>());

            // Wood table BG
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

            // ── Header: Swords icon filling behind "UPGRADES" text ──
            var headerContainer = new GameObject("Header", typeof(RectTransform));
            headerContainer.transform.SetParent(Root.transform, false);
            var headerRT = headerContainer.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.5f, 1f);
            headerRT.anchorMax = new Vector2(0.5f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(650, 130);
            headerRT.anchoredPosition = new Vector2(0, 10);

            if (art != null && art.swordsHeader != null)
            {
                var swordsImg = new GameObject("SwordsIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                swordsImg.transform.SetParent(headerContainer.transform, false);
                UIFactory.Stretch(swordsImg.GetComponent<RectTransform>());
                var si = swordsImg.GetComponent<Image>();
                si.sprite = art.swordsHeader;
                si.preserveAspect = true;
                si.raycastTarget = false;
            }

            var headerLabel = UIFactory.Label(headerContainer.transform, "UPGRADES", 36, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(headerLabel.rectTransform);

            // ── Coins (top right, same style as base HUD) ──
            var coinContainer = new GameObject("CoinContainer", typeof(RectTransform));
            coinContainer.transform.SetParent(Root.transform, false);
            var coinContainerRT = coinContainer.GetComponent<RectTransform>();
            UIFactory.Anchor(coinContainerRT, new Vector2(1, 1), new Vector2(220, 70), new Vector2(-15, -15));

            if (art != null && art.coinRibbon != null)
            {
                var ribbonImg = coinContainer.AddComponent<Image>();
                ribbonImg.sprite = art.coinRibbon;
                ribbonImg.type = Image.Type.Sliced;
                ribbonImg.preserveAspect = true;
            }

            var coinContent = new GameObject("CoinContent", typeof(RectTransform));
            coinContent.transform.SetParent(coinContainer.transform, false);
            var contentRT = coinContent.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = new Vector2(15, 5);
            contentRT.offsetMax = new Vector2(-15, -5);

            var hlg = coinContent.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            if (art != null && art.coinIcon != null)
            {
                var icon = UIFactory.Icon(coinContent.transform, art.coinIcon, new Vector2(36, 36));
                var le = icon.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 36;
                le.preferredHeight = 36;
            }

            _coinText = UIFactory.Label(coinContent.transform, "", 30, TextAnchor.MiddleLeft, Color.white);
            var coinLE = _coinText.gameObject.AddComponent<LayoutElement>();
            coinLE.flexibleWidth = 1;
            coinLE.preferredHeight = 40;

            var csf = coinContent.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── VOLVER button (bottom left, SmallRibbons 6) ──
            var volverSprite = art != null && art.ribbonVolver != null ? art.ribbonVolver : (art != null ? art.ribbonButton : null);
            if (volverSprite != null)
            {
                var volverBtn = UIFactory.ImageButton(Root.transform, volverSprite, new Vector2(260, 100), Hide);
                UIFactory.Anchor(volverBtn.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(260, 100), new Vector2(15, 15));
                var volverTxt = UIFactory.Label(volverBtn.transform, "VOLVER", 30, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Stretch(volverTxt.rectTransform);
            }
            else
            {
                var close = UIFactory.Button(Root.transform, "VOLVER", new Color(0.4f, 0.4f, 0.45f), Hide);
                UIFactory.Anchor(close.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(180, 60), new Vector2(15, 15));
            }

            // ── Tree viewport (drag + zoom) ──
            var viewport = UIFactory.Panel(Root.transform, "Viewport", new Color(0, 0, 0, 0));
            var vpRT = viewport.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = new Vector2(10, 60);
            vpRT.offsetMax = new Vector2(-10, -110);
            viewport.AddComponent<RectMask2D>();

            var zoomPan = viewport.AddComponent<TreeZoomPan>();

            var content = new GameObject("TreeContent", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            _treeContent = content.GetComponent<RectTransform>();
            _treeContent.anchorMin = _treeContent.anchorMax = _treeContent.pivot = new Vector2(0.5f, 0.5f);
            _treeContent.sizeDelta = new Vector2(3000, 2000);
            _treeContent.anchoredPosition = Vector2.zero;

            zoomPan.content = _treeContent;

            // ── Detail panel (right side, paper background) ──
            _detailPanel = new GameObject("DetailPanel", typeof(RectTransform));
            _detailPanel.transform.SetParent(Root.transform, false);
            var dpRT = _detailPanel.GetComponent<RectTransform>();
            dpRT.anchorMin = new Vector2(1, 0);
            dpRT.anchorMax = new Vector2(1, 1);
            dpRT.pivot = new Vector2(1, 0.5f);
            dpRT.sizeDelta = new Vector2(340, 0);
            dpRT.offsetMin = new Vector2(-340, 80);
            dpRT.offsetMax = new Vector2(-15, -120);

            var paperImg = _detailPanel.AddComponent<Image>();
            paperImg.color = new Color(0.92f, 0.87f, 0.75f, 0.95f);

            // Paper content
            var paperContent = new GameObject("PaperContent", typeof(RectTransform));
            paperContent.transform.SetParent(_detailPanel.transform, false);
            var pcRT = paperContent.GetComponent<RectTransform>();
            pcRT.anchorMin = Vector2.zero;
            pcRT.anchorMax = Vector2.one;
            pcRT.offsetMin = new Vector2(25, 25);
            pcRT.offsetMax = new Vector2(-25, -25);

            var vlg = paperContent.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(10, 10, 15, 15);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            _detailTitle = UIFactory.Label(paperContent.transform, "", 28, TextAnchor.MiddleCenter, new Color(0.2f, 0.15f, 0.1f));
            _detailTitle.fontStyle = FontStyle.Bold;
            _detailTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;

            _detailDesc = UIFactory.Label(paperContent.transform, "", 20, TextAnchor.UpperCenter, new Color(0.3f, 0.25f, 0.15f));
            _detailDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
            var descLE = _detailDesc.gameObject.AddComponent<LayoutElement>();
            descLE.preferredHeight = 80;
            descLE.flexibleHeight = 1;

            _detailCost = UIFactory.Label(paperContent.transform, "", 24, TextAnchor.MiddleCenter, new Color(0.6f, 0.45f, 0.1f));
            _detailCost.fontStyle = FontStyle.Bold;
            _detailCost.gameObject.AddComponent<LayoutElement>().preferredHeight = 35;

            _detailStatus = UIFactory.Label(paperContent.transform, "", 18, TextAnchor.MiddleCenter, new Color(0.5f, 0.2f, 0.1f));
            _detailStatus.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;

            // Buy button — ribbon style
            if (art != null && art.ribbonBuy != null)
            {
                _buyBtn = UIFactory.ImageButton(paperContent.transform, art.ribbonBuy, new Vector2(240, 70), OnBuy);
                var buyLE = _buyBtn.gameObject.AddComponent<LayoutElement>();
                buyLE.preferredHeight = 70;
                buyLE.preferredWidth = 240;
                _buyLabel = UIFactory.Label(_buyBtn.transform, "COMPRAR", 24, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Stretch(_buyLabel.rectTransform);
            }
            else
            {
                _buyBtn = UIFactory.Button(paperContent.transform, "COMPRAR", new Color(0.2f, 0.55f, 0.3f), OnBuy);
                var buyLE = _buyBtn.gameObject.AddComponent<LayoutElement>();
                buyLE.preferredHeight = 55;
                buyLE.preferredWidth = 200;
                _buyLabel = _buyBtn.GetComponentInChildren<Text>();
                _buyLabel.fontSize = 24;
            }

            _detailPanel.SetActive(false);

            Root.SetActive(false);
        }

        public void Show()
        {
            Root.SetActive(true);
            _detailPanel.SetActive(false);
            _selectedNode = null;
            Rebuild();
        }

        public void Hide() => Root.SetActive(false);

        private void Rebuild()
        {
            foreach (var nv in _nodeViews) UnityEngine.Object.Destroy(nv.go);
            foreach (var l in _lines) UnityEngine.Object.Destroy(l);
            _nodeViews.Clear();
            _lines.Clear();

            _coinText.text = $"{_ctx.State.wallet.coins}";

            var tree = _ctx.Config.upgradeTree;
            if (tree == null || tree.nodes == null || tree.nodes.Length == 0) return;

            var posMap = new Dictionary<string, Vector2>();
            foreach (var node in tree.nodes)
            {
                if (node == null) continue;
                posMap[node.id] = node.treePosition;
            }

            foreach (var node in tree.nodes)
            {
                if (node == null || node.prerequisiteIds == null) continue;
                if (!posMap.ContainsKey(node.id) || !IsVisible(node)) continue;

                foreach (var preId in node.prerequisiteIds)
                {
                    if (string.IsNullOrEmpty(preId) || !posMap.ContainsKey(preId)) continue;
                    var preNode = tree.Get(preId);
                    if (preNode != null && !IsVisible(preNode)) continue;

                    bool active = _ctx.State.upgrades.IsUnlocked(preId);
                    _lines.Add(CreateLine(posMap[preId], posMap[node.id], active));
                }
            }

            foreach (var node in tree.nodes)
            {
                if (node == null || !IsVisible(node)) continue;

                bool owned = _ctx.State.upgrades.IsUnlocked(node.id);
                var can = UpgradeService.CanPurchase(_ctx.State, node);
                _nodeViews.Add(CreateNodeView(node, posMap[node.id], owned, can.success));
            }

            RefreshDetail();
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

            // Sprite icon based on node type
            var iconSprite = GetNodeIcon(node);
            if (iconSprite != null)
            {
                var iconImg = UIFactory.Icon(nodeGO.transform, iconSprite, new Vector2(60, 60));
                var irt = iconImg.rectTransform;
                irt.anchorMin = irt.anchorMax = irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = Vector2.zero;
                iconImg.raycastTarget = false;
            }

            var btn = nodeGO.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1, 1, 1, 0.85f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;
            var cap = node;
            btn.onClick.AddListener(() => SelectNode(cap));

            return new NodeView { go = nodeGO, node = node };
        }

        private Sprite GetNodeIcon(UpgradeNode node)
        {
            if (_art == null) return null;

            // Recruit/roster/inheritance nodes
            if (node.effect == UpgradeEffectType.IncreaseRosterCap ||
                node.effect == UpgradeEffectType.InheritanceQualityBonus)
                return _art.nodeIconRecruits;

            // Economy nodes (base level, recruit quality, coins)
            if (node.effect == UpgradeEffectType.IncreaseBaseLevel ||
                node.effect == UpgradeEffectType.RecruitQualityBonus)
                return _art.nodeIconEconomy;

            // Class-specific by branch
            return node.branch switch
            {
                UpgradeBranch.Guerrero => _art.nodeIconWarrior,
                UpgradeBranch.Arquero => _art.nodeIconArcher,
                UpgradeBranch.Mago => _art.nodeIconMage,
                UpgradeBranch.Comandante => _art.nodeIconEconomy,
                _ => _art.nodeIconEconomy,
            };
        }

        private void SelectNode(UpgradeNode node)
        {
            _selectedNode = node;
            _detailPanel.SetActive(true);
            RefreshDetail();
        }

        private void RefreshDetail()
        {
            if (_selectedNode == null)
            {
                _detailPanel.SetActive(false);
                return;
            }

            var node = _selectedNode;
            bool owned = _ctx.State.upgrades.IsUnlocked(node.id);
            var can = UpgradeService.CanPurchase(_ctx.State, node);

            _detailTitle.text = node.displayName;
            _detailDesc.text = node.description;
            _detailCost.text = node.cost > 0 ? $"Coste: {node.cost} monedas" : "GRATIS";
            _detailStatus.text = "";

            if (owned)
            {
                _buyLabel.text = "COMPRADO";
                _buyBtn.interactable = false;
                _detailStatus.text = "Ya tienes esta mejora.";
            }
            else if (can.success)
            {
                _buyLabel.text = "COMPRAR";
                _buyBtn.interactable = true;
            }
            else
            {
                _buyLabel.text = "COMPRAR";
                _buyBtn.interactable = false;
                _detailStatus.text = can.reason switch
                {
                    UpgradeFailReason.SinMonedas => "Monedas insuficientes.",
                    UpgradeFailReason.NivelBaseInsuficiente => $"Requiere base nivel {node.requiredBaseLevel}.",
                    UpgradeFailReason.PrerequisitoFaltante => "Desbloquea el nodo anterior.",
                    _ => "No disponible.",
                };
            }
        }

        private void OnBuy()
        {
            if (_selectedNode == null) return;

            var result = UpgradeService.Purchase(_ctx.State, _selectedNode);
            if (result.success)
            {
                _ctx.Save();
                _onChanged?.Invoke();
            }
            Rebuild();
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

        private struct NodeView { public GameObject go; public UpgradeNode node; }
    }
}
