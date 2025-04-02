namespace _GAME.Scripts
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class BlockDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region FIELD_DECLARATIONS
        private Block  _block;
        private Camera _mainCamera;
        private Plane  _groundPlane;
        private Vector3 _dragOffset;

        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            this.OnInit();
        }
        public void OnDrag(PointerEventData    eventData)
        {
            HandleOnDrag(eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            this._block.Unhighlight();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var ray = this._mainCamera.ScreenPointToRay(eventData.position);
            if (this._groundPlane.Raycast(ray, out var distance))
            {
                var worldPoint = ray.GetPoint(distance);
                this._dragOffset = this.transform.position - worldPoint;
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

                var newPosition = new Vector3(worldPoint.x, this.transform.position.y, worldPoint.z);
                this.transform.position = newPosition;
            }
        }

    }
}