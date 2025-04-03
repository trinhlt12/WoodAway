namespace _GAME.Scripts
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class BlockDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region FIELD_DECLARATIONS
        private Block   _block;
        private Camera  _mainCamera;
        private Plane   _groundPlane;
        private Vector3 _dragOffset;
        private Vector3 _originalPosition;
        private Vector3 _lastValidPosition;
        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            this.OnInit();
        }

        private void Update()
        {
            if (!this._targetPosition.HasValue) return;
            var speed = 10f;
            this.transform.position = Vector3.MoveTowards(
                this.transform.position,
                this._targetPosition.Value,
                speed * Time.deltaTime
            );

            if (!(Vector3.Distance(this.transform.position, this._targetPosition.Value) < 0.005f/2)) return;
            this.transform.position = this._targetPosition.Value;
            this._targetPosition    = null;
        }

        public void OnDrag(PointerEventData    eventData)
        {
            HandleOnDrag(eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            HandleOnEndDrag(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var ray = this._mainCamera.ScreenPointToRay(eventData.position);
            if (this._groundPlane.Raycast(ray, out var distance))
            {
                var worldPoint = ray.GetPoint(distance);
                this._dragOffset   = this.transform.position - worldPoint;
                _originalPosition  = this.transform.position;
                _lastValidPosition = _originalPosition;

                GridManager.Instance.UnmarkCellsOccupied(this.transform.position, this._block.CellOffsets);

            }
            this._block.Highlight();
        }
        public void OnPointerUp(PointerEventData   eventData)
        {
            this._block.Unhighlight();
        }
        #endregion

        private void OnInit()
        {
            this._block = this.gameObject.GetComponent<Block>();
            this._mainCamera = Camera.main;
            this._groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        private Vector3? _targetPosition = null;

        private void HandleOnDrag(PointerEventData eventData)
        {
            this._block.Highlight();

            var ray = this._mainCamera.ScreenPointToRay(eventData.position);
            if (this._groundPlane.Raycast(ray, out var distance))
            {
                var worldPoint = ray.GetPoint(distance);
                worldPoint += this._dragOffset;

                var bounds = GridManager.Instance.CalculateGridBounds();
                var halfSize = this.transform.localScale.x / 2;

                /*worldPoint.x = Mathf.Clamp(worldPoint.x, bounds.min.x + halfSize, bounds.max.x - halfSize);
                worldPoint.z = Mathf.Clamp(worldPoint.z, bounds.min.z + halfSize, bounds.max.z - halfSize);*/

                var blockOffsets = this._block.CellOffsets;
                var minOffset = Vector3.positiveInfinity;
                var maxOffset = Vector3.negativeInfinity;

                foreach (var offset in blockOffsets)
                {
                    var cellWorldPosition = new Vector3(
                        worldPoint.x + offset.x,
                        0,
                        worldPoint.z + offset.y // y is z in world space
                    );

                    minOffset = Vector3.Min(minOffset, cellWorldPosition);
                    maxOffset = Vector3.Max(maxOffset, cellWorldPosition);
                }
                //Clamp the block to the grid:
                if(minOffset.x < bounds.min.x + halfSize)
                {
                    worldPoint.x += bounds.min.x + halfSize - minOffset.x;
                }
                if(maxOffset.x > bounds.max.x - halfSize)
                {
                    worldPoint.x -= maxOffset.x - bounds.max.x + halfSize;
                }
                if(minOffset.z < bounds.min.z + halfSize)
                {
                    worldPoint.z += bounds.min.z + halfSize - minOffset.z;
                }
                if(maxOffset.z > bounds.max.z - halfSize)
                {
                    worldPoint.z -= maxOffset.z - bounds.max.z + halfSize;
                }

                Vector2Int startCoord  = GridManager.Instance.GetGridCoordFromWorld(this.transform.position);
                Vector2Int targetCoord = GridManager.Instance.GetGridCoordFromWorld(worldPoint);

                var path = GridManager.Instance.FindPath(startCoord, targetCoord, _block.CellOffsets);

                if (path.Count > 1)
                {
                    var nextCoord    = path[1];
                    var nextWorldPos = GridManager.Instance.GetWorldFromGridCoord(nextCoord);

                    _targetPosition = new Vector3(
                        nextWorldPos.x,
                        this.transform.position.y,
                        nextWorldPos.z
                    );

                    _lastValidPosition = _targetPosition.Value;
                }
            }
        }

        private void HandleOnEndDrag(PointerEventData eventData)
        {
            this._block.Unhighlight();

            if (_targetPosition.HasValue)
            {
                this.transform.position = _targetPosition.Value;
                _targetPosition         = null;
            }

            var closestCell = GridManager.Instance.GetClosestCell(this.transform.position);
            if (closestCell != null)
            {
                var snapPosition = closestCell.position;
                snapPosition.y = this.transform.position.y;
                if (CanPlaceAtPosition(snapPosition))
                {
                    this.transform.position = snapPosition;
                    GridManager.Instance.MarkCellsOccupied(snapPosition, this._block.CellOffsets);
                    _lastValidPosition = snapPosition;
                }
                else
                {
                    var lastValidCell = GridManager.Instance.GetClosestCell(_lastValidPosition);
                    this.transform.position = lastValidCell.position;
                    GridManager.Instance.MarkCellsOccupied(this.transform.position, this._block.CellOffsets);
                    _lastValidPosition = this.transform.position;
                }
            }
        }


        private bool CanPlaceAtPosition(Vector3 position)
        {
            var pivotCoord   = GridManager.Instance.GetGridCoordFromWorld(position);
            var blockOffsets = this._block.CellOffsets;

            foreach (var offset in blockOffsets)
            {
                var coord = pivotCoord + new Vector2Int(offset.x, -offset.y);

                if (coord.x < 0 || coord.x >= 4 || coord.y < 0 || coord.y >= 5)
                    return false;

                var cell = GridManager.Instance.GetCell(coord.y, coord.x);
                if (cell.isOccupied)
                    return false;
            }

            return true;
        }


    }
}