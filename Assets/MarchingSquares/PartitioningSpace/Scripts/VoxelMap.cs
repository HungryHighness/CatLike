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
        private int _fillTypeIndex, _radiusIndex, _stencilIndex;
        private static readonly string[] FillTypeName = { "Filled", "Empty" };
        private static readonly string[] RadiusNames = { "0", "1", "2", "3", "4", "5" };
        private static readonly string[] StencilNames = { "Square", "Circle" };

        private readonly VoxelStencil[] _stencils = new[]
        {
            new VoxelStencil(),
            new VoxelStencilCircle()
        };

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
            int centerX = (int)((point.x + _halfSize) / _voxelSize);
            int centerY = (int)((point.y + _halfSize) / _voxelSize);

            int xStart = (centerX - _radiusIndex) / voxelResolution;
            if (xStart < 0)
            {
                xStart = 0;
            }

            int xEnd = (centerX + _radiusIndex) / voxelResolution;
            if (xEnd >= chunkResolution)
            {
                xEnd = chunkResolution - 1;
            }

            int yStart = (centerY - _radiusIndex) / voxelResolution;
            if (yStart < 0)
            {
                yStart = 0;
            }

            int yEnd = (centerY + _radiusIndex) / voxelResolution;
            if (yEnd >= chunkResolution)
            {
                yEnd = chunkResolution - 1;
            }


            VoxelStencil activeStencil = _stencils[_stencilIndex];
            activeStencil.Initialize(_fillTypeIndex == 0, _radiusIndex);

            int voxelYOffset = yStart * voxelResolution;
            for (int y = yStart; y <= yEnd; y++)
            {
                int i = y * chunkResolution + xStart;
                int voxelXOffset = xStart * voxelResolution;
                for (int x = xStart; x <= xEnd; x++, i++)
                {
                    activeStencil.SetCenter(centerX - voxelXOffset, centerY - voxelYOffset);
                    Debug.Log((centerX - voxelXOffset) + "and" + (centerY - voxelYOffset));
                    _chunks[i].Apply(activeStencil);
                    voxelXOffset += voxelResolution;
                }

                voxelYOffset += voxelResolution;
            }
        }

        private void CreateChunk(int i, int x, int y)
        {
            VoxelGrid chunk = Instantiate(voxelGridPrefab, transform, true);
            chunk.Initialize(voxelResolution, _chunkSize);
            chunk.transform.localPosition = new Vector3(x * _chunkSize - _halfSize, y * _chunkSize - _halfSize);
            _chunks[i] = chunk;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
            GUILayout.Label("Fill Type");
            _fillTypeIndex = GUILayout.SelectionGrid(_fillTypeIndex, FillTypeName, 2);
            GUILayout.Label("Radius");
            _radiusIndex = GUILayout.SelectionGrid(_radiusIndex, RadiusNames, 6);
            GUILayout.Label("Stencil");
            _stencilIndex = GUILayout.SelectionGrid(_stencilIndex, StencilNames, 2);

            GUILayout.EndArea();
        }
    }
}