using UnityEngine;

namespace MarchingSquares.PartitioningSpace.Scripts
{
    [SelectionBase]
    public class VoxelGrid : MonoBehaviour
    {
        public int resolution;
        public GameObject voxelPrefab;
        private bool[] _voxels;
        private float _voxelSize;
        private Material[] _voxelMaterials;

        public void Initialize(int resolution, float size)
        {
            this.resolution = resolution;
            _voxelSize = size / resolution;
            _voxels = new bool[resolution * resolution];
            _voxelMaterials = new Material[_voxels.Length];
            for (int i = 0, y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++, i++)
                {
                    CreateVoxel(i, x, y);
                }
            }

            SetVoxelColors();
        }

        private void SetVoxelColors()
        {
            for (var i = 0; i < _voxels.Length; i++)
            {
                _voxelMaterials[i].color = _voxels[i] ? Color.black : Color.white;
            }
        }

        private void CreateVoxel(int i, int x, int y)
        {
            GameObject o = Instantiate(voxelPrefab, transform, true);
            o.transform.localPosition = new Vector3((x + 0.5f) * _voxelSize, (y + 0.5f) * _voxelSize);
            o.transform.localScale = Vector3.one * _voxelSize * 0.9f;
            _voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
        }

        public void SetVoxel(int x, int y, bool state)
        {
            _voxels[y * resolution + x] = state;
            SetVoxelColors();
        }
    }
}