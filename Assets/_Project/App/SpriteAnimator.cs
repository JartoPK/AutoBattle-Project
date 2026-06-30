using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>
    /// Anima un SpriteRenderer ciclando frames (idle o run) a unos FPS fijos.
    /// Independiente de los Animator Controllers del asset (más fiable y nos deja
    /// alternar idle/run según el movimiento).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        public Sprite[] idle;
        public Sprite[] run;
        public float fps = 8f;
        public bool randomStartFrame; // para que las espumas no vayan sincronizadas

        private SpriteRenderer _sr;
        private bool _running;
        private float _timer;
        private int _frame;

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        private void Start()
        {
            if (idle != null && idle.Length > 0)
            {
                _frame = randomStartFrame ? Random.Range(0, idle.Length) : 0;
                _sr.sprite = idle[_frame];
            }
        }

        public void SetRunning(bool running)
        {
            if (running == _running) return;
            _running = running;
            _frame = 0;
            _timer = 0f;
        }

        private void Update()
        {
            var clip = (_running && run != null && run.Length > 0) ? run : idle;
            if (clip == null || clip.Length == 0) return;

            _timer += Time.deltaTime;
            if (_timer < 1f / fps) return;

            _timer = 0f;
            _frame = (_frame + 1) % clip.Length;
            _sr.sprite = clip[_frame];
        }
    }
}
