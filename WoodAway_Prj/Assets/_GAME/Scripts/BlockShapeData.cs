namespace _GAME.Scripts
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "BlockShapeData", menuName = "Block/BlockShape", order = 0)]
    public class BlockShapeData : ScriptableObject
    {
        [Tooltip("The cell child objects create the shape of the block. Offset is calculated from the center of the block (pivot).")]
        [SerializeField]
        private Vector2Int[] _cellOffsets;
        public Vector2Int[] CellOffsets => this._cellOffsets;
    }
}