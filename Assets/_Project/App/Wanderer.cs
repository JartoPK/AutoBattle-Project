using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>
    /// Mueve lentamente un objeto por el suelo de la base hacia puntos aleatorios,
    /// evitando las huellas de los edificios (obstáculos). Da vida visual a las
    /// tropas (cubos) en la base.
    /// </summary>
    public class Wanderer : MonoBehaviour
    {
        public struct Obstacle
        {
            public Vector2 center;
            public float radius;
        }

        public float radius = 11f;
        public float speed = 1.2f;
        public Obstacle[] obstacles;

        private float _y;
        private Vector3 _target;
        private float _wait;

        private void Start()
        {
            _y = transform.position.y;
            PickTarget();
        }

        private void Update()
        {
            if (_wait > 0f)
            {
                _wait -= Time.deltaTime;
                return;
            }

            var pos = transform.position;
            var next = Vector3.MoveTowards(pos, _target, speed * Time.deltaTime);

            // Si el siguiente paso entraría en un edificio, gira hacia otro punto.
            if (Blocked(next))
            {
                PickTarget();
                return;
            }

            transform.position = next;

            var dir = _target - pos;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir), 5f * Time.deltaTime);

            if ((next - _target).sqrMagnitude < 0.04f)
            {
                _wait = Random.Range(0.3f, 1.4f);
                PickTarget();
            }
        }

        private bool Blocked(Vector3 p)
        {
            if (obstacles == null) return false;
            var xz = new Vector2(p.x, p.z);
            foreach (var o in obstacles)
                if (Vector2.Distance(xz, o.center) < o.radius) return true;
            return false;
        }

        private void PickTarget()
        {
            for (int i = 0; i < 14; i++)
            {
                var c = new Vector3(Random.Range(-radius, radius), _y, Random.Range(-radius, radius));
                if (!Blocked(c)) { _target = c; return; }
            }
            _target = transform.position; // sin hueco libre: quédate quieto
        }
    }
}
