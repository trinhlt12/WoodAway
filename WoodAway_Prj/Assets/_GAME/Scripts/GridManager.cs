using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Transform gridRoot;

    private readonly Transform[,] _gridCells = new Transform[5, 4];

    private void Awake()
    {
        var index = 0;
        for(var row = 0; row < 5; row++)
        {
            for(var col = 0; col < 4; col++)
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
        if(_gridCells.Length == 0)
        {
            return new Bounds(gridRoot.position, Vector3.zero);
        }

        // Estimate cell size by measuring the distance between adjacent cells
        var cellSizeX = 1f;
        var cellSizeZ = 1f;

        // Use half size for extending bounds from center points
        var halfCellSizeX = cellSizeX / 2f;
        var halfCellSizeZ = cellSizeZ / 2f;

        var min    = this._gridCells[0, 0].position;
        var max    = this._gridCells[0, 0].position;
        for (var row = 0; row < 5; row++)
        {
            for (var col = 0; col < 4; col++)
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