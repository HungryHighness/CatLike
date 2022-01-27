using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MarchingSquares.PartitioningSpace.Scripts
{
    [SelectionBase]
    public class VoxelGrid : MonoBehaviour
    {
        public int resolution;
        public GameObject voxelPrefab;
        public VoxelGrid xNeighbor, yNeighbor, xyNeighbor;
        private Voxel[] _voxels;
        private Voxel _dummyX, _dummyY, _dummyT;
        private float _voxelSize, _gridSize;
        private Material[] _voxelMaterials;
        private Mesh _mesh;
        private List<Vector3> _vertices;
        private List<int> _triangles;

        public void Initialize(int resolution, float size)
        {
            this.resolution = resolution;
            _gridSize = size;
            _voxelSize = size / resolution;
            _voxels = new Voxel[resolution * resolution];
            _voxelMaterials = new Material[_voxels.Length];
            _dummyX = new Voxel();
            _dummyY = new Voxel();
            _dummyT = new Voxel();

            for (int i = 0, y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++, i++)
                {
                    CreateVoxel(i, x, y);
                }
            }

            GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
            _mesh.name = "VoxelGrid Mesh";
            _vertices = new List<Vector3>();
            _triangles = new List<int>();
            Refresh();
        }

        private void Refresh()
        {
            SetVoxelColors();
            Triangulate();
        }

        private void Triangulate()
        {
            _vertices.Clear();
            _triangles.Clear();
            _mesh.Clear();

            if (xNeighbor != null)
            {
                _dummyX.BecomeXDummyOf(xNeighbor._voxels[0], _gridSize);
            }

            TriangulateCeilRows();
            if (yNeighbor != null)
            {
                TriangulateGapRow();
            }

            _mesh.vertices = _vertices.ToArray();
            _mesh.triangles = _triangles.ToArray();
        }

        private void TriangulateCeilRows()
        {
            int cells = resolution - 1;
            for (int i = 0, y = 0; y < cells; y++, i++)
            {
                for (int x = 0; x < cells; x++, i++)
                {
                    TriangulateCell(_voxels[i], _voxels[i + 1], _voxels[i + resolution], _voxels[i + 1 + resolution]);
                }

                if (xNeighbor != null)
                {
                    TriangulateGapCell(i);
                }
            }
        }

        private void TriangulateGapCell(int i)
        {
            Voxel dummySwap = _dummyT;
            dummySwap.BecomeXDummyOf(xNeighbor._voxels[i + 1], _gridSize);
            _dummyT = _dummyX;
            _dummyX = dummySwap;
            TriangulateCell(_voxels[i], _dummyT, _voxels[i + resolution], _dummyX);
        }

        private void TriangulateGapRow()
        {
            _dummyY.BecomeYDummyOf(yNeighbor._voxels[0], _gridSize);
            int cells = resolution - 1;
            int offset = cells * resolution;

            for (int x = 0; x < cells; x++)
            {
                Voxel dummySwap = _dummyT;
                dummySwap.BecomeYDummyOf(yNeighbor._voxels[x + 1], _gridSize);
                _dummyT = _dummyY;
                _dummyY = dummySwap;
                TriangulateCell(_voxels[x + offset], _voxels[x + offset + 1], _dummyT, _dummyY);
            }

            if (xNeighbor != null)
            {
                _dummyT.BecomeXYDummyOf(xyNeighbor._voxels[0], _gridSize);
                TriangulateCell(_voxels[_voxels.Length - 1], _dummyX, _dummyY, _dummyT);
            }
        }

        private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d)
        {
            int cellType = 0;
            if (a.state)
            {
                cellType |= 1;
            }

            if (b.state)
            {
                cellType |= 2;
            }

            if (c.state)
            {
                cellType |= 4;
            }

            if (d.state)
            {
                cellType |= 8;
            }

            switch (cellType)
            {
                case 0:
                    return;
                case 1:
                    AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                    break;
                case 2:
                    AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                    break;
                case 3:
                    AddQuad(a.position, a.yEdgePosition, b.yEdgePosition, b.position);
                    break;
                case 4:
                    AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                    break;
                case 5:
                    AddQuad(a.position, c.position, c.xEdgePosition, a.xEdgePosition);
                    break;
                case 6:
                    AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                    AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                    break;
                case 7:
                    AddPentagon(a.position, c.position, c.xEdgePosition, b.yEdgePosition, b.position);
                    break;
                case 8:
                    AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                    break;
                case 9:
                    AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                    AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                    break;
                case 10:
                    AddQuad(a.xEdgePosition, c.xEdgePosition, d.position, b.position);
                    break;
                case 11:
                    AddPentagon(b.position, a.position, a.yEdgePosition, c.xEdgePosition, d.position);
                    break;
                case 12:
                    AddQuad(a.yEdgePosition, c.position, d.position, b.yEdgePosition);
                    break;
                case 13:
                    AddPentagon(c.position, d.position, b.yEdgePosition, a.xEdgePosition, a.position);
                    break;
                case 14:
                    AddPentagon(d.position, b.position, a.xEdgePosition, a.yEdgePosition, c.position);
                    break;
                case 15:
                    AddQuad(a.position, c.position, d.position, b.position);
                    break;
            }
        }

        private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
        }

        private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            _vertices.Add(d);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex + 3);
        }

        private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            _vertices.Add(d);
            _vertices.Add(e);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex + 3);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 3);
            _triangles.Add(vertexIndex + 4);
        }

        private void SetVoxelColors()
        {
            for (var i = 0; i < _voxels.Length; i++)
            {
                _voxelMaterials[i].color = _voxels[i].state ? Color.black : Color.white;
            }
        }

        private void CreateVoxel(int i, int x, int y)
        {
            GameObject o = Instantiate(voxelPrefab, transform, true);
            o.transform.localPosition = new Vector3((x + 0.5f) * _voxelSize, (y + 0.5f) * _voxelSize, -0.01f);
            o.transform.localScale = Vector3.one * _voxelSize * 0.1f;
            _voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
            _voxels[i] = new Voxel(x, y, _voxelSize);
        }

        public void SetVoxel(int x, int y, bool state)
        {
            _voxels[y * resolution + x].state = state;
            SetVoxelColors();
        }

        public void Apply(VoxelStencil stencil)
        {
            int xStart = stencil.XStart;
            if (xStart < 0)
            {
                xStart = 0;
            }

            int xEnd = stencil.XEnd;
            if (xEnd >= resolution)
            {
                xEnd = resolution - 1;
            }

            int yStart = stencil.YStart;
            if (yStart < 0)
            {
                yStart = 0;
            }

            int yEnd = stencil.YEnd;
            if (yEnd >= resolution)
            {
                yEnd = resolution - 1;
            }

            for (int y = yStart; y <= yEnd; y++)
            {
                int i = y * resolution + xStart;
                for (int x = xStart; x <= xEnd; x++, i++)
                {
                    _voxels[i].state = stencil.Apply(x, y, _voxels[i].state);
                }
            }

            Refresh();
        }

    }
}