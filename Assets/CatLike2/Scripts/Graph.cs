using UnityEngine;

namespace CatLike2.Scripts
{
    public class Graph : MonoBehaviour
    {
        [SerializeField] private Transform pointPrefab;

        [SerializeField, Range(10, 100)] private int resolution = 10;

        private Transform[] _points;
        private float _step;
        private Vector3 _scale;
        private Vector3 _position;

        // Start is called before the first frame update
        void Start()
        {
            _step = 2f / resolution;
            _scale = Vector3.one * _step;
            _position = Vector3.one;
            _points = new Transform[resolution];

            for (var i = 0; i < _points.Length; i++)
            {
                _points[i] = Instantiate(pointPrefab, transform, false);
                _position.x = (i + 0.5f) * _step - 1f;
                // _position.y = _position.x * _position.x * _position.x;
                _points[i].localPosition = _position;
                _points[i].localScale = _scale;
            }
        }

        // Update is called once per frame
        void Update()
        {
            float time = Time.time;
            for (var i = 0; i < _points.Length; i++)
            {
                _position = _points[i].position;
                _position.y = Mathf.Sin(Mathf.PI * (_position.x + time));
                _points[i].localPosition = _position;
            }
        }
    }
}