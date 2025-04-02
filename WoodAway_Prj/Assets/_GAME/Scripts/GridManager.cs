using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region FIELD_DECLARATIONS

    [SerializeField] private       Transform gridRoot;
    [SerializeField] private const int       columns = 4;
    [SerializeField] private const int       rows    = 5;

    public static GridManager Instance { get; private set; }
    private readonly Transform[,] _gridCells = new Transform[rows, columns];

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
        if (this.gridRoot == null) return;

        #if UNITY_EDITOR

        var index = 0;
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                if(this.gridRoot.childCount <= index) return;
                var cell = this.gridRoot.GetChild(index);
                var position = cell.position;

                Gizmos.DrawWireCube(position, new Vector3(1,0.01f, 1f));
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
                _gridCells[row, col] = gridRoot.GetChild(index);
                index++;
            }
        }
    }

    public Transform GetCell(int row, int col)
    {
        return _gridCells[row, col];
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

        var min = this._gridCells[0, 0].position;
        var max = this._gridCells[0, 0].position;
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                var position = this._gridCells[row, col].position;
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
}