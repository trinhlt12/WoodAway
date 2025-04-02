using UnityEngine;
using UnityEditor;
using _GAME.Scripts;

[CustomEditor(typeof(BlockShapeData))]
public class BlockShapeDataEditor : Editor
{
    private const int gridSize = 5;
    private const int cellSize = 20;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("🧩 Shape Preview", EditorStyles.boldLabel);

        BlockShapeData shapeData = (BlockShapeData)target;
        Vector2Int[]   offsets   = shapeData.CellOffsets;

        DrawGrid(offsets);
    }

    private void DrawGrid(Vector2Int[] offsets)
    {
        Rect rect = GUILayoutUtility.GetRect(gridSize * cellSize, gridSize * cellSize);
        Handles.BeginGUI();

        Vector2 center = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);

        for (int y = -gridSize / 2; y <= gridSize / 2; y++)
        {
            for (int x = -gridSize / 2; x <= gridSize / 2; x++)
            {
                Rect cellRect = new Rect(
                    center.x + x * cellSize,
                    center.y - y * cellSize,
                    cellSize, cellSize
                );

                Color color = Color.gray;

                if (x == 0 && y == 0)
                    color                                                               = Color.green;             // Pivot
                else if (System.Array.Exists(offsets, o => o.x == x && o.y == y)) color = new Color(1f, 0.6f, 0f); // Cell con

                EditorGUI.DrawRect(cellRect, color);
                Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, Color.black);
            }
        }

        Handles.EndGUI();
    }
}