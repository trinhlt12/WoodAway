using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridCell
{
    public Transform cellTransform;
    public bool      isOccupied;
}

public class GridManager : MonoBehaviour
{
    #region FIELD_DECLARATIONS

    [SerializeField] private        Transform gridRoot;
    [SerializeField] private static int       columns = 4;
    [SerializeField] private static int       rows    = 5;

    public static    GridManager Instance { get; private set; }
    private readonly GridCell[,] _gridCells = new GridCell[rows, columns];

    #endregion

    #region UNITY_CALLBACKS

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        this.OnInit();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (this.gridRoot == null || this._gridCells == null) return;

        #if UNITY_EDITOR

        var index = 0;
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                if (this.gridRoot.childCount <= index) return;
                var cell = this.gridRoot.GetChild(index);
                var pos  = cell.position;

                if (_gridCells[row, col] != null)
                {
                    if (_gridCells[row, col].isOccupied)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.green;
                }

                Gizmos.DrawCube(pos, new Vector3(0.9f, 0.05f, 0.9f));
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(pos, new Vector3(1, 0.01f, 1));
                index++;
            }
        }

        #endif
    }

    #endregion

    private void OnInit()
    {
        InitGrid();
    }

    private void InitGrid()
    {
        var index = 0;
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                this._gridCells[row, col] = new GridCell
                {
                    cellTransform = this.gridRoot.GetChild(index),
                    isOccupied    = false
                };
                index++;
            }
        }
    }

    public GridCell GetCell(int row, int col)
    {
        return _gridCells[row, col];
    }

    public Transform GetClosestCell(Vector3 worldPosition)
    {
        Transform closest     = null;
        var       minDistance = float.MaxValue;

        foreach (var cell in this._gridCells)
        {
            var distance = Vector3.Distance(worldPosition, cell.cellTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest     = cell.cellTransform;
            }
        }
        return closest;
    }

    public Bounds CalculateGridBounds()
    {
        if (_gridCells.Length == 0)
        {
            return new Bounds(gridRoot.position, Vector3.zero);
        }

        // Estimate cell size by measuring the distance between adjacent cells
        var cellSizeX = 1f;
        var cellSizeZ = 1f;

        // Use half size for extending bounds from center points
        var halfCellSizeX = cellSizeX / 2f;
        var halfCellSizeZ = cellSizeZ / 2f;

        var min = this._gridCells[0, 0].cellTransform.position;
        var max = this._gridCells[0, 0].cellTransform.position;
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                var position = this._gridCells[row, col].cellTransform.position;
                min = Vector3.Min(min, position);
                max = Vector3.Max(max, position);
            }
        }
        min -= new Vector3(halfCellSizeX, 0, halfCellSizeZ);
        max += new Vector3(halfCellSizeX, 0, halfCellSizeZ);

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    public void MarkCellsOccupied(Vector3 pivotWorldPos, Vector2Int[] offsets)
    {
        var pivotCoord = GetGridCoordFromWorld(pivotWorldPos);
        foreach (var offset in offsets)
        {
            var coord = pivotCoord + new Vector2Int(offset.x, -offset.y);
            if (coord.x < 0 || coord.x >= 4 || coord.y < 0 || coord.y >= 5) continue;
            _gridCells[coord.y, coord.x].isOccupied = true;
        }
    }

    public void UnmarkCellsOccupied(Vector3 pivotWorldPos, Vector2Int[] offsets)
    {
        var pivotCoord = GetGridCoordFromWorld(pivotWorldPos);
        foreach (var offset in offsets)
        {
            var coord = pivotCoord + new Vector2Int(offset.x, -offset.y);
            if (coord.x < 0 || coord.x >= 4 || coord.y < 0 || coord.y >= 5) continue;
            _gridCells[coord.y, coord.x].isOccupied = false;
        }
    }

    private void Start()
    {
        Debug.Log($"Grid bounds: {CalculateGridBounds()}");
        Debug.Log($"Grid width: {GetGridWidth()}");
        Debug.Log($"Grid length: {this.GetGridLength()}");
    }

    public float GetGridWidth()
    {
        return CalculateGridBounds().size.x;
    }

    public float GetGridLength()
    {
        return CalculateGridBounds().size.z;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Vector2Int[] draggedBlockOffsets)
    {
        var openList   = new List<Node>();
        var closedSet  = new HashSet<Vector2Int>();
        var gridWidth  = 4; // columns
        var gridHeight = 5; // rows

        Node startNode = new Node(start, null, 0, Heuristic(start, end));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            openList.Sort((a, b) => a.fCost.CompareTo(b.fCost));
            Node currentNode = openList[0];
            openList.RemoveAt(0);

            if (currentNode.position == end) return ReconstructPath(currentNode);

            closedSet.Add(currentNode.position);

            foreach (var direction in Directions)
            {
                Vector2Int neighborPos = currentNode.position + direction;

                if (neighborPos.x < 0 || neighborPos.x >= gridWidth || neighborPos.y < 0 || neighborPos.y >= gridHeight) continue;

                if (closedSet.Contains(neighborPos)) continue;

                var cell = GetCell(neighborPos.y, neighborPos.x);

                if (!IsBlockFitAtGridPosition(neighborPos, draggedBlockOffsets)) continue;

                int  tentativeG   = currentNode.gCost + 1;
                Node neighborNode = openList.FirstOrDefault(n => n.position == neighborPos);

                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos, currentNode, tentativeG, Heuristic(neighborPos, end));
                    openList.Add(neighborNode);
                }
                else if (tentativeG < neighborNode.gCost)
                {
                    neighborNode.parent = currentNode;
                    neighborNode.gCost  = tentativeG;
                    neighborNode.fCost  = neighborNode.gCost + neighborNode.hCost;
                }
            }
        }

        return new List<Vector2Int>();
    }

    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> ReconstructPath(Node endNode)
    {
        var  path    = new List<Vector2Int>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private class Node
    {
        public readonly Vector2Int position;
        public          Node       parent;
        public          int        gCost;
        public readonly int        hCost;
        public          int        fCost { get => gCost + hCost; set => fCost = value; }

        public Node(Vector2Int position, Node parent, int gCost, int hCost)
        {
            this.position = position;
            this.parent   = parent;
            this.gCost    = gCost;
            this.hCost    = hCost;
        }
    }

    public bool IsBlockFitAtGridPosition(Vector2Int pivotCoord, Vector2Int[] cellOffsets)
    {
        foreach (var offset in cellOffsets)
        {
            var coord = pivotCoord + new Vector2Int(offset.x, -offset.y);

            if (coord.x < 0 || coord.x >= 4 || coord.y < 0 || coord.y >= 5) return false;

            if (_gridCells[coord.y, coord.x].isOccupied) return false;
        }

        return true;
    }

    public Vector2Int GetGridCoordFromWorld(Vector3 worldPos)
    {
        float      minDistance  = float.MaxValue;
        Vector2Int closestCoord = new Vector2Int(-1, -1);

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                var   cell = _gridCells[row, col];
                float dist = Vector3.Distance(worldPos, cell.cellTransform.position);
                if (dist < minDistance)
                {
                    minDistance  = dist;
                    closestCoord = new Vector2Int(col, row);
                }
            }
        }

        return closestCoord;
    }

    public Vector3 GetWorldFromGridCoord(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= 4 || coord.y < 0 || coord.y >= 5) return Vector3.zero;
        return _gridCells[coord.y, coord.x].cellTransform.position;
    }
}