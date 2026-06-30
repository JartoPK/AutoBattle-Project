using System;
using System.Collections.Generic;
using AutoBattle.Core.Units;
using AutoBattle.Meta.Recruitment;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    public class RecruitPanel
    {
        public readonly GameObject Root;

        private readonly GameContext _ctx;
        private readonly Action _onChanged;
        private readonly ArtConfig _art;
        private readonly RarityRoulette _roulette;
        private readonly Text _coinText;
        private readonly Text _rosterText;
        private readonly Text _status;
        private readonly Transform _listContent;
        private readonly ScrollRect _listScroll;

        private bool _spinning;

        public RecruitPanel(Transform canvas, GameContext ctx, ArtConfig art, Action onChanged)
        {
            _ctx = ctx;
            _art = art;
            _onChanged = onChanged;

            // ── Full-screen root ──
            Root = UIFactory.Panel(canvas, "RecruitPanel", new Color(0.08f, 0.06f, 0.04f, 1f));
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

            // ── Header ribbon with title ──
            if (art != null && art.swordsHeader != null)
            {
                var headerContainer = new GameObject("Header", typeof(RectTransform));
                headerContainer.transform.SetParent(Root.transform, false);
                var headerRT = headerContainer.GetComponent<RectTransform>();
                headerRT.anchorMin = new Vector2(0.5f, 1f);
                headerRT.anchorMax = new Vector2(0.5f, 1f);
                headerRT.pivot = new Vector2(0.5f, 1f);
                headerRT.sizeDelta = new Vector2(650, 130);
                headerRT.anchoredPosition = new Vector2(0, 10);

                var ribbonImg = new GameObject("Ribbon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                ribbonImg.transform.SetParent(headerContainer.transform, false);
                UIFactory.Stretch(ribbonImg.GetComponent<RectTransform>());
                var ri = ribbonImg.GetComponent<Image>();
                ri.sprite = art.swordsHeader;
                ri.preserveAspect = true;
                ri.raycastTarget = false;

                var headerLabel = UIFactory.Label(headerContainer.transform, "RECLUTAMIENTO", 34, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Stretch(headerLabel.rectTransform);
            }
            else
            {
                var title = UIFactory.Label(Root.transform, "RECLUTAMIENTO", 44, TextAnchor.UpperCenter, Color.white);
                UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1), new Vector2(500, 60), new Vector2(0, -20));
            }

            // ── Coins (top right) ──
            var coinContainer = new GameObject("CoinContainer", typeof(RectTransform));
            coinContainer.transform.SetParent(Root.transform, false);
            UIFactory.Anchor(coinContainer.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(220, 70), new Vector2(-15, -15));

            if (art != null && art.coinRibbon != null)
            {
                var cImg = coinContainer.AddComponent<Image>();
                cImg.sprite = art.coinRibbon;
                cImg.type = Image.Type.Sliced;
                cImg.preserveAspect = true;
            }

            var coinContent = new GameObject("CoinContent", typeof(RectTransform));
            coinContent.transform.SetParent(coinContainer.transform, false);
            var ccRT = coinContent.GetComponent<RectTransform>();
            ccRT.anchorMin = Vector2.zero; ccRT.anchorMax = Vector2.one;
            ccRT.offsetMin = new Vector2(15, 5); ccRT.offsetMax = new Vector2(-15, -5);

            var coinHlg = coinContent.AddComponent<HorizontalLayoutGroup>();
            coinHlg.spacing = 6; coinHlg.childAlignment = TextAnchor.MiddleCenter;
            coinHlg.childForceExpandWidth = false; coinHlg.childForceExpandHeight = false;

            if (art != null && art.coinIcon != null)
            {
                var icon = UIFactory.Icon(coinContent.transform, art.coinIcon, new Vector2(36, 36));
                var le = icon.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 36; le.preferredHeight = 36;
            }

            _coinText = UIFactory.Label(coinContent.transform, "", 30, TextAnchor.MiddleLeft, Color.white);
            var coinLE = _coinText.gameObject.AddComponent<LayoutElement>();
            coinLE.flexibleWidth = 1; coinLE.preferredHeight = 40;
            coinContent.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Roster info (top left) ──
            _rosterText = UIFactory.Label(Root.transform, "", 26, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.7f));
            UIFactory.Anchor(_rosterText.rectTransform, new Vector2(0, 1), new Vector2(300, 40), new Vector2(20, -20));

            // ── Tier buttons row (below header) ──
            var tierRow = UIFactory.Panel(Root.transform, "Tiers", new Color(0, 0, 0, 0));
            var trt = tierRow.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 1); trt.anchorMax = new Vector2(0.5f, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.sizeDelta = new Vector2(900, 90);
            trt.anchoredPosition = new Vector2(0, -130);
            var hlg = tierRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            var tiers = _ctx.Config.recruitmentConfig != null ? _ctx.Config.recruitmentConfig.tiers : null;
            if (tiers != null)
            {
                for (int i = 0; i < tiers.Length; i++)
                {
                    int index = i;
                    var tier = tiers[i];
                    var label = tier.cost > 0 ? $"{tier.displayName}\n({tier.cost} mon.)" : tier.displayName;

                    if (art != null && art.ribbonBuy != null)
                    {
                        var btn = UIFactory.ImageButton(tierRow.transform, art.ribbonBuy, new Vector2(200, 80), () => Recruit(index));
                        var txt = UIFactory.Label(btn.transform, label, 20, TextAnchor.MiddleCenter, Color.white);
                        UIFactory.Stretch(txt.rectTransform);
                    }
                    else
                    {
                        UIFactory.Button(tierRow.transform, label, new Color(0.2f, 0.55f, 0.3f), () => Recruit(index));
                    }
                }
            }

            // ── Status text ──
            _status = UIFactory.Label(Root.transform, "", 22, TextAnchor.LowerLeft, new Color(1, 0.9f, 0.6f));
            UIFactory.Anchor(_status.rectTransform, new Vector2(0, 0), new Vector2(900, 35), new Vector2(20, 15));

            // ── VOLVER button (bottom left) ──
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

            // ── Troop list (scrollable, center area) ──
            var listViewport = UIFactory.Panel(Root.transform, "ListViewport", new Color(0, 0, 0, 0.15f));
            var lvRT = listViewport.GetComponent<RectTransform>();
            lvRT.anchorMin = Vector2.zero; lvRT.anchorMax = Vector2.one;
            lvRT.offsetMin = new Vector2(20, 65);
            lvRT.offsetMax = new Vector2(-20, -230);
            listViewport.AddComponent<RectMask2D>();

            var scroll = listViewport.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            _listScroll = scroll;

            var listContainer = new GameObject("ListContent", typeof(RectTransform));
            listContainer.transform.SetParent(listViewport.transform, false);
            var lcRT = listContainer.GetComponent<RectTransform>();
            lcRT.anchorMin = new Vector2(0, 1); lcRT.anchorMax = new Vector2(1, 1);
            lcRT.pivot = new Vector2(0.5f, 1);
            lcRT.sizeDelta = new Vector2(0, 0);

            var vlg = listContainer.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 10, 10); vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            var csf = listContainer.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = lcRT;
            _listContent = listContainer.transform;

            // Ruleta
            var rouletteGo = new GameObject("RarityRouletteRunner");
            rouletteGo.transform.SetParent(canvas, false);
            _roulette = rouletteGo.AddComponent<RarityRoulette>();
            _roulette.Init(canvas);

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
            if (_spinning) return;

            var rc = _ctx.Config.recruitmentConfig;
            if (rc == null || rc.tiers == null || tierIndex >= rc.tiers.Length) return;
            var tier = rc.tiers[tierIndex];

            var result = RecruitmentService.Recruit(_ctx.State, tier, rc, _ctx.Config.baseConfig);
            if (!result.success)
            {
                _status.text = result.reason switch
                {
                    RecruitFailReason.RosterLleno => "Roster lleno.",
                    RecruitFailReason.SinMonedas => "No tienes monedas suficientes.",
                    _ => $"No se pudo reclutar ({result.reason}).",
                };
                return;
            }

            _ctx.Save();

            var pool = new List<Rarity>();
            if (tier.rarityTable != null)
                foreach (var rw in tier.rarityTable) pool.Add(rw.rarity);

            _spinning = true;
            _status.text = "";
            _roulette.Play(pool, result.unit.rarity, () =>
            {
                _spinning = false;
                _status.text = $"¡Reclutado {result.unit.displayName} [{RarityVisuals.Name(result.unit.rarity)}]!";
                _onChanged?.Invoke();
                Refresh();
            });
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
            _coinText.text = $"{_ctx.State.wallet.coins}";
            _rosterText.text = $"Tropas: {_ctx.State.roster.Count}/{cap}";

            for (int i = _listContent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_listContent.GetChild(i).gameObject);

            foreach (var u in _ctx.State.roster.units)
            {
                if (u.hasClass)
                {
                    var row = UIFactory.Panel(_listContent, "Row", new Color(1, 1, 1, 0.08f));
                    row.AddComponent<LayoutElement>().preferredHeight = 55;

                    var rowHlg = row.AddComponent<HorizontalLayoutGroup>();
                    rowHlg.padding = new RectOffset(12, 12, 6, 6); rowHlg.spacing = 10;
                    rowHlg.childAlignment = TextAnchor.MiddleLeft;
                    rowHlg.childControlWidth = rowHlg.childControlHeight = true;
                    rowHlg.childForceExpandWidth = false; rowHlg.childForceExpandHeight = true;

                    // Class icon
                    var classIcon = GetClassIcon(u.classId);
                    if (classIcon != null)
                    {
                        var ic = UIFactory.Icon(row.transform, classIcon, new Vector2(40, 40));
                        ic.gameObject.AddComponent<LayoutElement>().preferredWidth = 40;
                    }

                    var lbl = UIFactory.Label(row.transform,
                        $"[{RarityVisuals.Name(u.rarity)}] {u.classId} · {u.displayName} — {Stats(u)}",
                        20, TextAnchor.MiddleLeft, RarityVisuals.Of(u.rarity));
                    var lblLE = lbl.gameObject.AddComponent<LayoutElement>();
                    lblLE.flexibleWidth = 1;
                }
                else
                {
                    var row = UIFactory.Panel(_listContent, "Row", new Color(1, 1, 1, 0.08f));
                    row.AddComponent<LayoutElement>().preferredHeight = 100;
                    var col = row.AddComponent<VerticalLayoutGroup>();
                    col.padding = new RectOffset(12, 12, 6, 6); col.spacing = 6;
                    col.childAlignment = TextAnchor.UpperLeft;
                    col.childControlWidth = col.childControlHeight = true;
                    col.childForceExpandWidth = true; col.childForceExpandHeight = false;

                    var lbl = UIFactory.Label(row.transform,
                        $"[{RarityVisuals.Name(u.rarity)}] {u.displayName} — {Stats(u)} · SIN CLASE:",
                        20, TextAnchor.MiddleLeft, RarityVisuals.Of(u.rarity));
                    lbl.gameObject.AddComponent<LayoutElement>().preferredHeight = 34;

                    var btnRow = UIFactory.Panel(row.transform, "Btns", new Color(0, 0, 0, 0));
                    btnRow.AddComponent<LayoutElement>().preferredHeight = 50;
                    var hl = btnRow.AddComponent<HorizontalLayoutGroup>();
                    hl.spacing = 12; hl.childAlignment = TextAnchor.MiddleLeft;
                    hl.childControlWidth = hl.childControlHeight = true;
                    hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;

                    string id = u.id;
                    ClassButton(btnRow.transform, "Guerrero", id, UnitClass.Guerrero);
                    ClassButton(btnRow.transform, "Arquero", id, UnitClass.Arquero);
                    ClassButton(btnRow.transform, "Mago", id, UnitClass.Mago);
                }
            }
        }

        private static string Stats(UnitInstance u)
        {
            var s = u.baseStats;
            var line = $"HP {s.hp:F0}, ATK {s.attack:F1}, SPD {s.attackSpeed:F2}/s";
            if (s.mana > 0f) line += $", Maná {s.mana:F0}";
            return line;
        }

        private void ClassButton(Transform parent, string label, string unitId, UnitClass cls)
        {
            var classIcon = GetClassIcon(cls);
            if (_art != null && _art.ribbonBuy != null)
            {
                var btn = UIFactory.ImageButton(parent, _art.ribbonBuy, new Vector2(160, 48), () => Assign(unitId, cls));
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 160; le.preferredHeight = 48;
                var txt = UIFactory.Label(btn.transform, label, 20, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Stretch(txt.rectTransform);
            }
            else
            {
                var b = UIFactory.Button(parent, label, new Color(0.25f, 0.4f, 0.6f), () => Assign(unitId, cls));
                var le = b.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 150; le.preferredHeight = 44;
            }
        }

        private Sprite GetClassIcon(UnitClass cls)
        {
            if (_art == null) return null;
            return cls switch
            {
                UnitClass.Guerrero => _art.nodeIconWarrior,
                UnitClass.Arquero => _art.nodeIconArcher,
                UnitClass.Mago => _art.nodeIconMage,
                _ => null,
            };
        }
    }
}
