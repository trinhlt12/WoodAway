using UnityEngine;
using UnityEditor;
using _GAME.Scripts;

[CustomEditor(typeof(BlockShapeData))]
public class BlockShapeDataEditor : Editor
{
    private const int _gridSize = 5;
    private const int _cellSize = 20;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("🧩 Shape Preview", EditorStyles.boldLabel);

        var shapeData = (BlockShapeData)target;
        var   offsets   = shapeData.CellOffsets;

        DrawGrid(offsets);
    }

    private static void DrawGrid(Vector2Int[] offsets)
    {
        var rect = GUILayoutUtility.GetRect(_gridSize * _cellSize, _gridSize * _cellSize);
        Handles.BeginGUI();

        var center = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);

        for (var y = -_gridSize / 2; y <= _gridSize / 2; y++)
        {
            for (var x = -_gridSize / 2; x <= _gridSize / 2; x++)
            {
                var cellRect = new Rect(
                    center.x + x * _cellSize,
                    center.y - y * _cellSize,
                    _cellSize, _cellSize
                );

                var color = Color.gray;

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