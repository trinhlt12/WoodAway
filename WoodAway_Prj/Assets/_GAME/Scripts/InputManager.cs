using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public bool CanAcceptInput { get; private set; } = true;

    public SwipeDirection CurrentSwipeDirection { get => CanAcceptInput ? _currentSwipeDirection : SwipeDirection.None; private set => _currentSwipeDirection = value; }

    private PlayerInputActions _inputActions;
    private SwipeDirection     _currentSwipeDirection;

    private Vector2 _swipeStart;
    private Vector2 _currentTouchPosition;
    private bool _isTouching = false;

    private const float SwipeThreshold = 50f; // pixels

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeInput();
    }

    private void OnEnable()
    {
        if (_inputActions != null && !_inputActions.asset.enabled)
        {
            _inputActions.Enable();
        }
    }

    private void OnDisable()
    {
        if (_inputActions != null)
        {
            _inputActions.Disable();
        }
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            UnsubscribeFromEvents();
            _inputActions.Dispose();
            _inputActions = null;
        }
    }

    private void InitializeInput()
    {
        _inputActions = new PlayerInputActions();
        SetupTouchActions();
        _inputActions.Enable();
    }

    private void SetupTouchActions()
    {
        _inputActions.Player.TouchPress.started  += HandleTouchStart;
        _inputActions.Player.TouchPress.canceled += HandleTouchEnd;

        // For mouse/touch position tracking
        _inputActions.Player.Touch.performed     += HandleTouchPositionChange;
    }

    private void UnsubscribeFromEvents()
    {
        if (_inputActions == null) return;

        _inputActions.Player.TouchPress.started  -= HandleTouchStart;
        _inputActions.Player.TouchPress.canceled -= HandleTouchEnd;
        _inputActions.Player.Touch.performed     -= HandleTouchPositionChange;
    }

    private void HandleTouchStart(InputAction.CallbackContext ctx)
    {
        if (!CanAcceptInput) return;

        // Get the current touch position
        _currentTouchPosition = GetTouchPosition();
        _swipeStart = _currentTouchPosition;
        _isTouching = true;

        // Reset swipe direction when starting a new touch
        CurrentSwipeDirection = SwipeDirection.None;
    }

    private void HandleTouchPositionChange(InputAction.CallbackContext ctx)
    {
        if (!_isTouching || !CanAcceptInput) return;

        _currentTouchPosition = GetTouchPosition();
    }

    private void HandleTouchEnd(InputAction.CallbackContext ctx)
    {
        if (!_isTouching || !CanAcceptInput) return;

        // Final swipe calculation on touch end
        Vector2 swipeDelta = _currentTouchPosition - _swipeStart;
        DetectSwipeDirection(swipeDelta);

        _isTouching = false;
    }

    private Vector2 GetTouchPosition()
    {
        // For testing in editor with mouse
        if (Touchscreen.current == null || !Touchscreen.current.primaryTouch.press.isPressed)
        {
            // Fallback to mouse position when testing in editor
            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();
            return Vector2.zero;
        }

        return Touchscreen.current.primaryTouch.position.ReadValue();
    }

    private void DetectSwipeDirection(Vector2 delta)
    {
        if (delta.magnitude < SwipeThreshold)
        {
            // Keep the previous direction
            return;
        }

        bool isHorizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
        CurrentSwipeDirection = isHorizontal
            ? (delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left)
            : (delta.y > 0 ? SwipeDirection.Forward : SwipeDirection.Backward);

        Debug.Log($"Swipe detected: {CurrentSwipeDirection}, Delta: {delta}");
    }

    public void ResetSwipeDirection()
    {
        CurrentSwipeDirection = SwipeDirection.None;
    }

    public void SetInputActive(bool active)
    {
        CanAcceptInput = active;
        if (!active)
        {
            ResetSwipeDirection();
        }
    }
}

public enum SwipeDirection
{
    None     = 0,
    Left     = 1,
    Right    = 2,
    Forward  = 3,
    Backward = 4
}