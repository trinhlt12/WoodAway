using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridGenerator : MonoBehaviour
{
    [Header("Grid Configuration")] [SerializeField] private Transform  gridRoot;
    [SerializeField]                                private GameObject cellPrefab;
    public int        columns  = 5;
    public int        rows     = 10;
    [SerializeField]                                private float      cellSize = 1f;
    [SerializeField]                                private float      spacing  = 0.1f;

    [Header("Options")] [SerializeField] private bool centerGrid  = true;
    [SerializeField]                     private bool alignToGrid = true;

    public void GenerateGrid()
    {
        if (gridRoot == null)
        {
            Debug.LogError("Grid Root is not assigned!");
            return;
        }

        if (cellPrefab == null)
        {
            Debug.LogError("Cell Prefab is not assigned!");
            return;
        }

        // Clear existing grid
        ClearGrid();

        // Calculate starting position
        var startX = centerGrid ? -(columns * (cellSize + spacing) - spacing) / 2f : 0f;
        var startZ = centerGrid ? -(rows * (cellSize + spacing) - spacing) / 2f : 0f;

        // Generate new grid
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                var position = new Vector3(
                    startX + col * (cellSize + spacing),
                    0f,
                    startZ + row * (cellSize + spacing)
                );

                var cellObj = Instantiate(cellPrefab, position, Quaternion.identity, gridRoot);
                cellObj.name = $"Cell_{row}_{col}";

                if (alignToGrid)
                {
                    cellObj.transform.localScale = new Vector3(cellSize, cellObj.transform.localScale.y, cellSize);
                }
            }
        }

        // Find and reset GridManager if it exists
        var gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            // Force re-initialization on next enable by disabling and enabling
            gridManager.gameObject.SetActive(false);
            gridManager.gameObject.SetActive(true);
        }
    }

    public void ClearGrid()
    {
        if (gridRoot == null) return;

        // Using while loop to clear all children
        while (gridRoot.childCount > 0)
        {
            DestroyImmediate(gridRoot.GetChild(0).gameObject);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridGenerator generator = (GridGenerator)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate Grid", GUILayout.Height(30)))
        {
            generator.GenerateGrid();
        }

        if (GUILayout.Button("Clear Grid", GUILayout.Height(30)))
        {
            generator.ClearGrid();
        }
    }
}
#endif