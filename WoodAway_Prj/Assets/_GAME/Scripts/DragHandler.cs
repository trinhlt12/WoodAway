namespace _GAME.Scripts
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class DragHandler : MonoBehaviour
    {
        private Camera _mainCamera;
        private Block _currentTouchedBlock;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            DetectBlockTouch();
        }

        private void DetectBlockTouch()
        {
            if(!InputManager.Instance.CanAcceptInput)
            {
                return;
            }

            //Check if touch/click started this frame
            if (InputManager.Instance.GetTouchDown())
            {
                /*if(EventSystem.current != null || EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }*/

                var touchPos = InputManager.Instance.GetTouchPos();
                var ray = this._mainCamera.ScreenPointToRay(touchPos);

                if (Physics.Raycast(ray, out var hit))
                {
                    var block = hit.collider.GetComponentInParent<Block>();
                    if (block != null)
                    {
                        _currentTouchedBlock = block;
                        HandleBlockTouched(block);
                    }
                }
            }

            //Handle touch edn
            if (InputManager.Instance.GetTouchUp())
            {
                if (_currentTouchedBlock != null)
                {
                    HandleBlockReleased(_currentTouchedBlock);
                    _currentTouchedBlock = null;
                }
            }
        }

        private void HandleBlockReleased(Block currentTouchedBlock)
        {
            Debug.Log($"Block {currentTouchedBlock.gameObject.name} released");
            UnhighlightBlock(currentTouchedBlock);
        }

        private void HandleBlockTouched(Block block)
        {
            Debug.Log($"Block {block.gameObject.name} touched");
            HighlightBlock(block);
        }

        private void HighlightBlock(Block block)
        {
            block.Highlight();
        }

        private void UnhighlightBlock(Block block)
        {
            block.Unhighlight();
        }
    }
}