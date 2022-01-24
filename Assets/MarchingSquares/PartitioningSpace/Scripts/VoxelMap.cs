using UnityEngine;

namespace MarchingSquares.PartitioningSpace.Scripts
{
    public class VoxelMap : MonoBehaviour
    {
        public float size = 2f;

        public int voxelResolution = 8;
        public int chunkResolution = 2;

        public VoxelGrid voxelGridPrefab;

        private VoxelGrid[] _chunks;

        private float _chunkSize, _voxelSize, _halfSize;

        private void Awake()
        {
            _halfSize = size * 0.5f;
            _chunkSize = size / chunkResolution;
            _voxelSize = _chunkSize / voxelResolution;

            _chunks = new VoxelGrid[chunkResolution * chunkResolution];
            for (int i = 0, y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++, i++)
                {
                    CreateChunk(i, x, y);
                }
            }

            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(size, size);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Camera.main != null &&
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo))
                {
                    if (hitInfo.collider.gameObject == gameObject)
                    {
                        EditVoxels(transform.InverseTransformPoint(hitInfo.point));
                    }
                }
            }
        }

        private void EditVoxels(Vector3 point)
        {
            int voxelX = (int)((point.x + _halfSize) / _voxelSize);
            int voxelY = (int)((point.y + _halfSize) / _voxelSize);
            int chunkX = voxelX / voxelResolution;
            int chunkY = voxelY / voxelResolution;
            voxelX -= chunkX * voxelResolution;
            voxelY -= chunkY * voxelResolution;
            _chunks[chunkY * chunkResolution + chunkX].SetVoxel(voxelX, voxelY, true);
            // Debug.Log(point.x + "," + point.y + "," + _voxelSize);
            // Debug.Log(point.x + _halfSize + "," + point.y + _halfSize);
            Debug.Log(voxelX + ", " + voxelY + " in chunk " + chunkX + ", " + chunkY);
        }

        private void CreateChunk(int i, int x, int y)
        {
            VoxelGrid chunk = Instantiate(voxelGridPrefab, transform, true);
            chunk.Initialize(voxelResolution, _chunkSize);
            chunk.transform.localPosition = new Vector3(x * _chunkSize - _halfSize, y * _chunkSize - _halfSize);
            _chunks[i] = chunk;
        }
    }
}