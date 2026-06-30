using UnityEngine;
using UnityEngine.Tilemaps;

namespace AutoBattle.App
{
    public class Wanderer2D : MonoBehaviour
    {
        public struct Obstacle { public Vector2 center; public float radius; }

        public float areaX = 7f;
        public float areaY = 3.2f;
        public float speed = 1.2f;
        public Obstacle[] obstacles;
        public Tilemap noWalkTilemap;

        private SpriteRenderer _sr;
        private SpriteAnimator _anim;
        private Vector2 _target;
        private float _wait;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            _anim = GetComponent<SpriteAnimator>();
            Pick();
        }

        private void Update()
        {
            bool moving = false;

            if (_wait > 0f) { _wait -= Time.deltaTime; }
            else
            {
                Vector2 pos = transform.position;
                Vector2 next = Vector2.MoveTowards(pos, _target, speed * Time.deltaTime);

                if (Blocked(next)) { Pick(); }
                else
                {
                    moving = (next - pos).sqrMagnitude > 1e-6f;
                    transform.position = new Vector3(next.x, next.y, 0f);
                    if (_sr != null)
                    {
                        if (next.x < pos.x - 0.001f) _sr.flipX = true;
                        else if (next.x > pos.x + 0.001f) _sr.flipX = false;
                    }
                    if ((next - _target).sqrMagnitude < 0.01f) { _wait = Random.Range(0.3f, 1.4f); Pick(); }
                }
            }

            if (_anim != null) _anim.SetRunning(moving);
            if (_sr != null) _sr.sortingOrder = 1500 + Mathf.RoundToInt(-transform.position.y * 100f);
        }

        private bool Blocked(Vector2 p)
        {
            if (obstacles != null)
                foreach (var o in obstacles)
                    if (Vector2.Distance(p, o.center) < o.radius) return true;

            if (noWalkTilemap != null)
            {
                var cell = noWalkTilemap.WorldToCell(new Vector3(p.x, p.y, 0f));
                if (noWalkTilemap.HasTile(cell)) return true;
            }

            return false;
        }

        private void Pick()
        {
            for (int i = 0; i < 14; i++)
            {
                var c = new Vector2(Random.Range(-areaX, areaX), Random.Range(-areaY, areaY));
                if (!Blocked(c)) { _target = c; return; }
            }
            _target = transform.position;
        }
    }
}
