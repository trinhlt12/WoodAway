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

                var bounds = GridManager.Instance.CalculateGridBounds();

                worldPoint.x = Mathf.Clamp(worldPoint.x, bounds.min.x, bounds.max.x);
                worldPoint.z = Mathf.Clamp(worldPoint.z, bounds.min.z, bounds.max.z);

                var newPosition = new Vector3(worldPoint.x, this.transform.position.y, worldPoint.z);
                this.transform.position = newPosition;
            }
        }

    }
}