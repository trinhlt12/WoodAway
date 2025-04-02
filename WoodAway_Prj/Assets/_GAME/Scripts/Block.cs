using System;
using _GAME.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour
{
    [SerializeField] private Outline _outline;
    [SerializeField] private BlockShapeData _shapeData;
    public Vector2Int[] CellOffsets => this._shapeData.CellOffsets;

    private void Awake()
    {
        this.OnInit();
    }

    private void Start()
    {
        GridManager.Instance.MarkCellsOccupied(this.transform.position, this.CellOffsets);
    }

    private void OnInit()
    {
        this._outline.enabled = false;
    }

    public void Highlight()
    {
        this._outline.enabled = true;
    }

    public void Unhighlight()
    {
        this._outline.enabled = false;
    }
}