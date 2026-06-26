using System;
using AutoBattle.Meta.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    /// <summary>
    /// Modal 2D del árbol de mejoras. Muestra los nodos agrupados por rama
    /// (Comandante + clases) con su estado: comprado / comprable / bloqueado.
    /// Comprar aplica el efecto (UpgradeService) y refresca el panel.
    /// </summary>
    public class UpgradeTreePanel
    {
        public readonly GameObject Root;

        private readonly GameContext _ctx;
        private readonly Action _onChanged;
        private readonly Text _info;
        private readonly Text _status;
        private readonly Transform _content;

        public UpgradeTreePanel(Transform canvas, GameContext ctx, Action onChanged)
        {
            _ctx = ctx;
            _onChanged = onChanged;

            Root = UIFactory.Panel(canvas, "UpgradeTreePanel", new Color(0f, 0f, 0f, 0.6f));
            UIFactory.Stretch(Root.GetComponent<RectTransform>());

            var panel = UIFactory.Panel(Root.transform, "Panel", new Color(0.12f, 0.13f, 0.18f, 0.98f));
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(1280, 840);
            prt.anchoredPosition = Vector2.zero;

            var title = UIFactory.Label(panel.transform, "Árbol de mejoras", 44, TextAnchor.UpperLeft, Color.white);
            UIFactory.Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(600, 60), new Vector2(30, -20));

            _info = UIFactory.Label(panel.transform, "", 28, TextAnchor.UpperRight, new Color(1, 1, 1, 0.85f));
            UIFactory.Anchor(_info.rectTransform, new Vector2(1, 1), new Vector2(620, 50), new Vector2(-30, -28));

            var contentPanel = UIFactory.Panel(panel.transform, "Content", new Color(0, 0, 0, 0));
            var crt = contentPanel.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(30, 90); crt.offsetMax = new Vector2(-30, -90);
            var vlg = contentPanel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10; vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            _content = contentPanel.transform;

            _status = UIFactory.Label(panel.transform, "", 24, TextAnchor.MiddleLeft, new Color(1, 0.9f, 0.6f));
            UIFactory.Anchor(_status.rectTransform, new Vector2(0, 0), new Vector2(900, 40), new Vector2(30, 30));

            var close = UIFactory.Button(panel.transform, "CERRAR", new Color(0.4f, 0.4f, 0.45f), Hide);
            UIFactory.Anchor(close.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(200, 80), new Vector2(-30, 25));

            Root.SetActive(false);
        }

        public void Show()
        {
            Root.SetActive(true);
            _status.text = "";
            Refresh();
        }

        public void Hide() => Root.SetActive(false);

        private void Refresh()
        {
            _info.text = $"Base nivel {_ctx.State.baseLevel}   ·   Monedas: {_ctx.State.wallet.coins}";

            for (int i = _content.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_content.GetChild(i).gameObject);

            var tree = _ctx.Config.upgradeTree;
            if (tree == null) { _status.text = "No hay árbol configurado."; return; }

            foreach (UpgradeBranch branch in Enum.GetValues(typeof(UpgradeBranch)))
            {
                var header = UIFactory.Label(_content, BranchName(branch), 28, TextAnchor.MiddleLeft, new Color(0.8f, 0.85f, 1f));
                header.gameObject.AddComponent<LayoutElement>().preferredHeight = 36;

                var rowPanel = UIFactory.Panel(_content, "Row", new Color(1, 1, 1, 0.04f));
                rowPanel.AddComponent<LayoutElement>().preferredHeight = 132;
                var hlg = rowPanel.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 12; hlg.padding = new RectOffset(12, 12, 8, 8);
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

                foreach (var node in tree.GetByBranch(branch))
                    CreateNodeButton(rowPanel.transform, node);
            }
        }

        private void CreateNodeButton(Transform parent, UpgradeNode node)
        {
            bool owned = _ctx.State.upgrades.IsUnlocked(node.id);
            var can = UpgradeService.CanPurchase(_ctx.State, node);

            Color color = owned
                ? new Color(0.22f, 0.5f, 0.3f)
                : (can.success ? new Color(0.2f, 0.45f, 0.65f) : new Color(0.3f, 0.3f, 0.34f));

            string text = $"{node.displayName}\n{node.cost} mon.";
            if (owned) text += "\n(comprado)";
            else if (node.requiredBaseLevel > 1) text += $"\n(req. base {node.requiredBaseLevel})";

            var btn = UIFactory.Button(parent, text, color, () => OnNode(node));
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 165;
            le.preferredHeight = 116;

            // El texto se ajusta DENTRO del botón (no se sale ni se solapa con el vecino).
            var label = btn.GetComponentInChildren<Text>();
            label.fontSize = 18;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private void OnNode(UpgradeNode node)
        {
            if (_ctx.State.upgrades.IsUnlocked(node.id))
            {
                _status.text = $"'{node.displayName}' ya está comprado.";
                return;
            }

            var result = UpgradeService.Purchase(_ctx.State, node);
            if (result.success)
            {
                _ctx.Save();
                _status.text = $"Comprado: {node.displayName}.";
                _onChanged?.Invoke();
            }
            else
            {
                _status.text = result.reason switch
                {
                    UpgradeFailReason.SinMonedas => "No tienes monedas suficientes.",
                    UpgradeFailReason.NivelBaseInsuficiente => $"Requiere nivel de base {node.requiredBaseLevel}.",
                    UpgradeFailReason.PrerequisitoFaltante => "Te falta un nodo previo.",
                    UpgradeFailReason.YaDesbloqueado => "Ya está comprado.",
                    _ => "No se puede comprar ahora.",
                };
            }

            Refresh();
        }

        private static string BranchName(UpgradeBranch branch) => branch switch
        {
            UpgradeBranch.Comandante => "Comandante",
            UpgradeBranch.Guerrero => "Guerrero",
            UpgradeBranch.Arquero => "Arquero",
            UpgradeBranch.Mago => "Mago",
            _ => branch.ToString(),
        };
    }
}
