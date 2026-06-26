using System;
using AutoBattle.Core.Units;
using AutoBattle.Meta.Recruitment;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    /// <summary>
    /// Modal 2D que se abre al tocar el edificio de Reclutamiento. Permite reclutar
    /// por tier (la tropa sale SIN clase) y asignar la clase a cada tropa sin clase.
    /// </summary>
    public class RecruitPanel
    {
        public readonly GameObject Root;

        private readonly GameContext _ctx;
        private readonly Action _onChanged;
        private readonly Text _info;
        private readonly Text _status;
        private readonly Transform _listContent;

        public RecruitPanel(Transform canvas, GameContext ctx, Action onChanged)
        {
            _ctx = ctx;
            _onChanged = onChanged;

            // Fondo oscuro (modal): bloquea clics al mundo de detrás.
            Root = UIFactory.Panel(canvas, "RecruitPanel", new Color(0f, 0f, 0f, 0.6f));
            UIFactory.Stretch(Root.GetComponent<RectTransform>());

            var panel = UIFactory.Panel(Root.transform, "Panel", new Color(0.12f, 0.13f, 0.18f, 0.98f));
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(1150, 780);
            prt.anchoredPosition = Vector2.zero;

            var title = UIFactory.Label(panel.transform, "Reclutamiento", 44, TextAnchor.UpperLeft, Color.white);
            UIFactory.Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(500, 60), new Vector2(30, -20));

            _info = UIFactory.Label(panel.transform, "", 28, TextAnchor.UpperRight, new Color(1, 1, 1, 0.85f));
            UIFactory.Anchor(_info.rectTransform, new Vector2(1, 1), new Vector2(560, 50), new Vector2(-30, -28));

            // Fila de botones de tier.
            var tierRow = UIFactory.Panel(panel.transform, "Tiers", new Color(0, 0, 0, 0));
            var trt = tierRow.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1); trt.pivot = new Vector2(0.5f, 1);
            trt.sizeDelta = new Vector2(-60, 120); trt.anchoredPosition = new Vector2(0, -85);
            var hlg = tierRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14; hlg.padding = new RectOffset(30, 30, 0, 0);
            hlg.childControlWidth = hlg.childControlHeight = true;
            hlg.childForceExpandWidth = hlg.childForceExpandHeight = true;

            var tiers = _ctx.Config.recruitmentConfig != null ? _ctx.Config.recruitmentConfig.tiers : null;
            if (tiers != null)
            {
                for (int i = 0; i < tiers.Length; i++)
                {
                    int index = i;
                    var tier = tiers[i];
                    var label = tier.cost > 0 ? $"{tier.displayName}\n({tier.cost} mon.)" : tier.displayName;
                    UIFactory.Button(tierRow.transform, label, new Color(0.2f, 0.55f, 0.3f), () => Recruit(index));
                }
            }

            // Lista de tropas.
            var listPanel = UIFactory.Panel(panel.transform, "List", new Color(0, 0, 0, 0.25f));
            var lrt = listPanel.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(30, 90); lrt.offsetMax = new Vector2(-30, -220);
            var vlg = listPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12); vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            _listContent = listPanel.transform;

            _status = UIFactory.Label(panel.transform, "", 24, TextAnchor.MiddleLeft, new Color(1, 0.9f, 0.6f));
            UIFactory.Anchor(_status.rectTransform, new Vector2(0, 0), new Vector2(800, 40), new Vector2(30, 30));

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

        private void Recruit(int tierIndex)
        {
            var rc = _ctx.Config.recruitmentConfig;
            if (rc == null || rc.tiers == null || tierIndex >= rc.tiers.Length) return;

            var result = RecruitmentService.Recruit(_ctx.State, rc.tiers[tierIndex], rc, _ctx.Config.baseConfig);
            if (result.success)
            {
                _ctx.Save();
                _status.text = $"Reclutado {result.unit.displayName} [{result.unit.rarity}] — asígnale una clase abajo.";
            }
            else
            {
                _status.text = result.reason switch
                {
                    RecruitFailReason.RosterLleno => "Roster lleno.",
                    RecruitFailReason.SinMonedas => "No tienes monedas suficientes.",
                    _ => $"No se pudo reclutar ({result.reason}).",
                };
            }

            _onChanged?.Invoke();
            Refresh();
        }

        private void Assign(string unitId, UnitClass cls)
        {
            if (ClassAssignmentService.Assign(_ctx.State, unitId, cls, _ctx.Config.recruitmentConfig))
            {
                _ctx.Save();
                _status.text = $"Clase asignada: {cls}.";
            }
            _onChanged?.Invoke();
            Refresh();
        }

        private void Refresh()
        {
            int cap = _ctx.State.GetRosterCap(_ctx.Config.baseConfig);
            _info.text = $"Tropas: {_ctx.State.roster.Count}/{cap}   ·   Monedas: {_ctx.State.wallet.coins}";

            for (int i = _listContent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_listContent.GetChild(i).gameObject);

            foreach (var u in _ctx.State.roster.units)
            {
                var row = UIFactory.Panel(_listContent, "Row", new Color(1, 1, 1, 0.05f));
                row.AddComponent<LayoutElement>().preferredHeight = 50;
                var hl = row.AddComponent<HorizontalLayoutGroup>();
                hl.spacing = 8; hl.padding = new RectOffset(10, 10, 0, 0);
                hl.childAlignment = TextAnchor.MiddleLeft;
                hl.childControlWidth = hl.childControlHeight = true;
                hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;

                if (u.hasClass)
                {
                    var lbl = UIFactory.Label(row.transform,
                        $"[{u.rarity}] {u.classId} · {u.displayName} — HP {u.baseStats.hp:F0}, ATQ {u.baseStats.attack:F1}, pasiva '{u.passiveId}'",
                        24, TextAnchor.MiddleLeft, Color.white);
                    lbl.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
                }
                else
                {
                    var lbl = UIFactory.Label(row.transform,
                        $"[{u.rarity}] {u.displayName} — HP {u.baseStats.hp:F0}, ATQ {u.baseStats.attack:F1} · SIN CLASE:",
                        24, TextAnchor.MiddleLeft, new Color(1, 0.9f, 0.6f));
                    lbl.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

                    string id = u.id;
                    ClassButton(row.transform, "Guerrero", id, UnitClass.Guerrero);
                    ClassButton(row.transform, "Arquero", id, UnitClass.Arquero);
                    ClassButton(row.transform, "Mago", id, UnitClass.Mago);
                }
            }
        }

        private void ClassButton(Transform parent, string label, string unitId, UnitClass cls)
        {
            var b = UIFactory.Button(parent, label, new Color(0.25f, 0.4f, 0.6f), () => Assign(unitId, cls));
            var le = b.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 150;
            le.preferredHeight = 44;
        }
    }
}
