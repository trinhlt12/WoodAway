using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler
{
    [SerializeField] private Outline             _outline;
    private                  IPointerDownHandler pointerDownHandlerImplementation;

    private void Awake()
    {
        this.OnInit();
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

    public void OnDrag(PointerEventData eventData)
    {
        this.Highlight();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        this.Highlight();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        this.Unhighlight();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.Unhighlight();
    }
}