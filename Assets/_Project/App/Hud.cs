using System;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    public class Hud
    {
        private readonly GameContext _ctx;
        private readonly Text _coins;
        private readonly GameObject _baseBtn;
        private readonly GameObject _backBtn;

        public Hud(Transform canvas, GameContext ctx, ArtConfig art, Action onOpenBase, Action onBack)
        {
            _ctx = ctx;

            // ── Coin display (ribbon + icon + number, right-aligned) ──
            var coinContainer = new GameObject("CoinContainer", typeof(RectTransform));
            coinContainer.transform.SetParent(canvas, false);
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

            _coins = UIFactory.Label(coinContent.transform, "", 30, TextAnchor.MiddleLeft, Color.white);
            var coinLE = _coins.gameObject.AddComponent<LayoutElement>();
            coinLE.flexibleWidth = 1;
            coinLE.preferredHeight = 40;

            var csf = coinContent.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Buttons ──
            if (art != null && art.ribbonButton != null)
            {
                var baseBtn = UIFactory.ImageButton(canvas, art.ribbonButton, new Vector2(260, 100), onOpenBase);
                UIFactory.Anchor(baseBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(260, 100), new Vector2(-15, 15));
                var baseTxt = UIFactory.Label(baseBtn.transform, "BASE", 30, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Stretch(baseTxt.rectTransform);
                _baseBtn = baseBtn.gameObject;

                var backBtn = UIFactory.ImageButton(canvas, art.ribbonButton, new Vector2(260, 100), onBack);
                UIFactory.Anchor(backBtn.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(260, 100), new Vector2(15, 15));
                var backTxt = UIFactory.Label(backBtn.transform, "MAPA", 30, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Stretch(backTxt.rectTransform);
                _backBtn = backBtn.gameObject;
            }
            else
            {
                var baseBtn = UIFactory.Button(canvas, "BASE", new Color(0.95f, 0.6f, 0.1f), onOpenBase);
                UIFactory.Anchor(baseBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(260, 100), new Vector2(-15, 15));
                _baseBtn = baseBtn.gameObject;

                var backBtn = UIFactory.Button(canvas, "MAPA", new Color(0.4f, 0.4f, 0.45f), onBack);
                UIFactory.Anchor(backBtn.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(260, 100), new Vector2(15, 15));
                _backBtn = backBtn.gameObject;
            }
        }

        public void SetContext(bool inBase)
        {
            _baseBtn.SetActive(!inBase);
            _backBtn.SetActive(inBase);
        }

        public void Refresh() => _coins.text = $"{_ctx.State.wallet.coins}";
    }
}
