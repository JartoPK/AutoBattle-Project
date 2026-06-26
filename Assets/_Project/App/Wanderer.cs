using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>
    /// Mueve lentamente un objeto por el suelo de la base hacia puntos aleatorios,
    /// con pequeñas pausas. Da vida visual a las tropas (cubos) en la base.
    /// </summary>
    public class Wanderer : MonoBehaviour
    {
        public float radius = 12f;
        public float speed = 1.2f;

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

        private void PickTarget() =>
            _target = new Vector3(Random.Range(-radius, radius), _y, Random.Range(-radius, radius));
    }
}
