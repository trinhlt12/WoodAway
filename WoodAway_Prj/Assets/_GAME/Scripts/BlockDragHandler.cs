namespace _GAME.Scripts
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class BlockDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Block _block;

        private void Awake()
        {
            this._block = this.gameObject.GetComponent<Block>();
        }

        public void OnDrag(PointerEventData    eventData)
        {
            this._block.Highlight();
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
    }
}