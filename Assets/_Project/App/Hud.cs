using System;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    /// <summary>HUD 2D persistente: monedas y los botones de contexto (BASE / VOLVER).</summary>
    public class Hud
    {
        private readonly GameContext _ctx;
        private readonly Text _coins;
        private readonly GameObject _baseBtn;
        private readonly GameObject _backBtn;

        public Hud(Transform canvas, GameContext ctx, Action onOpenBase, Action onBack)
        {
            _ctx = ctx;

            _coins = UIFactory.Label(canvas, "", 38, TextAnchor.UpperRight, new Color(1f, 0.85f, 0.3f));
            UIFactory.Anchor(_coins.rectTransform, new Vector2(1, 1), new Vector2(420, 60), new Vector2(-40, -30));

            var baseBtn = UIFactory.Button(canvas, "BASE", new Color(0.95f, 0.6f, 0.1f), onOpenBase);
            UIFactory.Anchor(baseBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(240, 120), new Vector2(-40, 40));
            _baseBtn = baseBtn.gameObject;

            var backBtn = UIFactory.Button(canvas, "VOLVER", new Color(0.4f, 0.4f, 0.45f), onBack);
            UIFactory.Anchor(backBtn.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(220, 100), new Vector2(40, 40));
            _backBtn = backBtn.gameObject;
        }

        public void SetContext(bool inBase)
        {
            _baseBtn.SetActive(!inBase);
            _backBtn.SetActive(inBase);
        }

        public void Refresh() => _coins.text = $"Monedas: {_ctx.State.wallet.coins}";
    }
}
