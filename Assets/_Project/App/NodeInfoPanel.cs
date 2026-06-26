using AutoBattle.Core.Units;
using AutoBattle.Meta.Campaign;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    /// <summary>
    /// Modal 2D con la info de un nodo del mapa: tipo, dificultad, recompensa y una
    /// imagen de prueba de la formación enemiga (rejilla de cuadros por clase).
    /// El combate aún no existe (lo hace otra persona), así que "Combatir" es solo
    /// un marcador.
    /// </summary>
    public class NodeInfoPanel
    {
        public readonly GameObject Root;

        private readonly Text _title;
        private readonly Text _meta;
        private readonly Text _reward;
        private readonly Text _status;
        private readonly RectTransform _formation;

        public NodeInfoPanel(Transform canvas)
        {
            Root = UIFactory.Panel(canvas, "NodeInfoPanel", new Color(0f, 0f, 0f, 0.6f));
            UIFactory.Stretch(Root.GetComponent<RectTransform>());

            var panel = UIFactory.Panel(Root.transform, "Panel", new Color(0.12f, 0.13f, 0.18f, 0.98f));
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(940, 700);
            prt.anchoredPosition = Vector2.zero;

            _title = UIFactory.Label(panel.transform, "", 42, TextAnchor.UpperLeft, Color.white);
            UIFactory.Anchor(_title.rectTransform, new Vector2(0, 1), new Vector2(880, 56), new Vector2(30, -22));

            _meta = UIFactory.Label(panel.transform, "", 28, TextAnchor.UpperLeft, new Color(1, 1, 1, 0.85f));
            UIFactory.Anchor(_meta.rectTransform, new Vector2(0, 1), new Vector2(880, 40), new Vector2(30, -82));

            _reward = UIFactory.Label(panel.transform, "", 28, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.4f));
            UIFactory.Anchor(_reward.rectTransform, new Vector2(0, 1), new Vector2(880, 40), new Vector2(30, -124));

            var formationTitle = UIFactory.Label(panel.transform, "Formación enemiga:", 26, TextAnchor.UpperLeft, new Color(0.8f, 0.85f, 1f));
            UIFactory.Anchor(formationTitle.rectTransform, new Vector2(0, 1), new Vector2(880, 36), new Vector2(30, -176));

            var board = UIFactory.Panel(panel.transform, "Board", new Color(0.08f, 0.09f, 0.12f, 1f));
            _formation = board.GetComponent<RectTransform>();
            _formation.anchorMin = new Vector2(0.5f, 1f);
            _formation.anchorMax = new Vector2(0.5f, 1f);
            _formation.pivot = new Vector2(0.5f, 1f);
            _formation.sizeDelta = new Vector2(420, 320);
            _formation.anchoredPosition = new Vector2(0, -220);

            _status = UIFactory.Label(panel.transform, "", 22, TextAnchor.MiddleLeft, new Color(1, 1, 1, 0.6f));
            UIFactory.Anchor(_status.rectTransform, new Vector2(0, 0), new Vector2(560, 40), new Vector2(30, 28));

            var combat = UIFactory.Button(panel.transform, "COMBATIR\n(próximamente)", new Color(0.3f, 0.35f, 0.4f),
                () => _status.text = "El combate lo construye la otra persona — aún no disponible.");
            UIFactory.Anchor(combat.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(280, 90), new Vector2(-250, 25));

            var close = UIFactory.Button(panel.transform, "CERRAR", new Color(0.4f, 0.4f, 0.45f), Hide);
            UIFactory.Anchor(close.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(200, 90), new Vector2(-30, 25));

            Root.SetActive(false);
        }

        public void Show(CampaignNodeData node)
        {
            Root.SetActive(true);
            _status.text = "";

            _title.text = node.displayName;
            _meta.text = $"Tipo: {TypeName(node.type)}    ·    Dificultad: {node.difficulty}/5";
            _reward.text = $"Recompensa: {RewardText(node)}";

            BuildFormation(node);
        }

        public void Hide() => Root.SetActive(false);

        private void BuildFormation(CampaignNodeData node)
        {
            for (int i = _formation.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_formation.GetChild(i).gameObject);

            if (node.formation == null) return;

            const float cell = 84f, gap = 8f;
            foreach (var enemy in node.formation)
            {
                var go = UIFactory.Panel(_formation, "Enemy", ClassColor(enemy.unitClass));
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1); // origen arriba-izquierda
                rt.sizeDelta = new Vector2(cell - gap, cell - gap);
                rt.anchoredPosition = new Vector2(16 + enemy.col * cell, -16 - enemy.row * cell);

                var lbl = UIFactory.Label(go.transform, ClassInitial(enemy.unitClass), 34, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Stretch(lbl.rectTransform);
            }
        }

        private static string RewardText(CampaignNodeData node) => node.type switch
        {
            NodeType.Reclutamiento => "Una tropa nueva",
            NodeType.Recursos => $"{node.coinReward} monedas (recursos)",
            _ => $"{node.coinReward} monedas",
        };

        private static string TypeName(NodeType type) => type switch
        {
            NodeType.Combate => "Combate",
            NodeType.Elite => "Élite",
            NodeType.Jefe => "Jefe",
            NodeType.Reclutamiento => "Reclutamiento",
            NodeType.Recursos => "Recursos",
            _ => type.ToString(),
        };

        private static Color ClassColor(UnitClass cls) => cls switch
        {
            UnitClass.Guerrero => new Color(0.8f, 0.35f, 0.3f),
            UnitClass.Arquero => new Color(0.35f, 0.7f, 0.4f),
            UnitClass.Mago => new Color(0.4f, 0.5f, 0.85f),
            _ => Color.gray,
        };

        private static string ClassInitial(UnitClass cls) => cls switch
        {
            UnitClass.Guerrero => "G",
            UnitClass.Arquero => "A",
            UnitClass.Mago => "M",
            _ => "?",
        };
    }
}
