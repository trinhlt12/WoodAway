using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour
{
    [SerializeField] private Outline             _outline;
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
}