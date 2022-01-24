using System;
using UnityEngine;

namespace MarchingSquares.PartitioningSpace
{
    [SelectionBase]
    public class VoxelGrid : MonoBehaviour
    {
        public int resolution;
        public GameObject voxelPrefab;
        private bool[] _voxels;
        private float _voxelSize;

        public void Initialize(int resolution, float size)
        {
            this.resolution = resolution;
            _voxelSize = size / resolution;
            _voxels = new bool[resolution * resolution];
            for (int i = 0, y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++, i++)
                {
                    CreateVoxel(i, x, y);
                }
            }
        }

        private void CreateVoxel(int i, int x, int y)
        {
            GameObject o = Instantiate(voxelPrefab, transform, true);
            o.transform.localPosition = new Vector3((x + 0.5f) * _voxelSize, (y + 0.5f) * _voxelSize);
            o.transform.localScale = Vector3.one * _voxelSize * 0.9f;
        }

    }
}