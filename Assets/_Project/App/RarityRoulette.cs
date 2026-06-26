using System;
using System.Collections;
using System.Collections.Generic;
using AutoBattle.Core.Units;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    /// <summary>
    /// Ruleta visual de rareza: al reclutar gira rápido por las rarezas posibles
    /// del tier y desacelera hasta pararse en la que ha tocado. Es solo visual;
    /// el resultado ya está decidido por el RecruitmentService.
    /// </summary>
    public class RarityRoulette : MonoBehaviour
    {
        private GameObject _root;
        private Text _label;

        public void Init(Transform canvas)
        {
            _root = UIFactory.Panel(canvas, "RoulettePanel", new Color(0f, 0f, 0f, 0.75f));
            UIFactory.Stretch(_root.GetComponent<RectTransform>());

            var card = UIFactory.Panel(_root.transform, "Card", new Color(0.14f, 0.15f, 0.20f, 1f));
            var crt = card.GetComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(740, 360);
            crt.anchoredPosition = Vector2.zero;

            var head = UIFactory.Label(card.transform, "Reclutando...", 34, TextAnchor.UpperCenter, new Color(1, 1, 1, 0.8f));
            UIFactory.Anchor(head.rectTransform, new Vector2(0.5f, 1), new Vector2(640, 50), new Vector2(0, -26));

            _label = UIFactory.Label(card.transform, "", 80, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(_label.rectTransform);

            _root.SetActive(false);
        }

        public void Play(List<Rarity> pool, Rarity result, Action onDone)
        {
            _root.SetActive(true);
            StartCoroutine(Spin(pool, result, onDone));
        }

        private IEnumerator Spin(List<Rarity> pool, Rarity result, Action onDone)
        {
            if (pool != null && pool.Count > 0)
            {
                var rng = new System.Random();
                float elapsed = 0f, duration = 1.4f;

                while (elapsed < duration)
                {
                    SetLabel(pool[rng.Next(pool.Count)]);
                    float interval = Mathf.Lerp(0.04f, 0.20f, elapsed / duration); // desacelera
                    yield return new WaitForSeconds(interval);
                    elapsed += interval;
                }
            }

            SetLabel(result);
            yield return new WaitForSeconds(0.6f);

            _root.SetActive(false);
            onDone?.Invoke();
        }

        private void SetLabel(Rarity rarity)
        {
            _label.text = RarityVisuals.Name(rarity);
            _label.color = RarityVisuals.Of(rarity);
        }
    }
}
