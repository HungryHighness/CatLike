using UnityEngine;
using UnityEngine.Serialization;

namespace CatLike3.Scripts
{
    public class Graph : MonoBehaviour
    {
        [SerializeField] private Transform pointPrefab;

        [SerializeField, Range(10, 100)] private int resolution = 10;

        [SerializeField] private FunctionLibrary.FunctionName functionName = default;
        private Transform[] _points;


        // Start is called before the first frame update
        void Start()
        {
            float step = 2f / resolution;
            var scale = Vector3.one * step;
            _points = new Transform[resolution * resolution];

            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = Instantiate(pointPrefab, transform, false);
                _points[i].localScale = scale;
            }
        }

        // Update is called once per frame
        void Update()
        {
            float step = 2f / resolution;
            float time = Time.time;
            float v = 0.5f * step - 1f;
            FunctionLibrary.Function f = FunctionLibrary.GetFunction(functionName);
            for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
            {
                if (x == resolution)
                {
                    x = 0;
                    z += 1;
                    v = (z + 0.5f) * step - 1f;
                }

                float u = (x + 0.5f) * step - 1f;
                _points[i].localPosition = f(u, v, time);
            }
        }
    }
}