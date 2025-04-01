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
    private Vector2 _swipeEnd;

    private const float SwipeThreshold = 50f; // pixels

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate InputManager found. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeInput();
    }

    private void InitializeInput()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        SetupTouchActions();
    }

    private void SetupTouchActions()
    {
        _inputActions.Player.TouchPress.started  += HandleSwipeStart;
        _inputActions.Player.Touch.performed     += ctx => _swipeEnd = ctx.ReadValue<Vector2>();
        _inputActions.Player.TouchPress.canceled += HandleSwipeEnd;
    }

    private void HandleSwipeStart(InputAction.CallbackContext ctx)
    {
        _swipeStart = _inputActions.Player.Touch.ReadValue<Vector2>();
        _swipeEnd   = _swipeStart;
    }

    private void HandleSwipeEnd(InputAction.CallbackContext ctx)
    {
        Vector2 swipeDelta = _swipeEnd - _swipeStart;
        DetectSwipeDirection(swipeDelta);
    }

    private void DetectSwipeDirection(Vector2 delta)
    {
        if (delta.magnitude < SwipeThreshold)
        {
            CurrentSwipeDirection = SwipeDirection.None;
            return;
        }

        bool isHorizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
        CurrentSwipeDirection = isHorizontal
            ? (delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left)
            : (delta.y > 0 ? SwipeDirection.Forward : SwipeDirection.Backward);

        Debug.Log($"Swipe detected: {CurrentSwipeDirection}");
    }

    public void ResetSwipeDirection()
    {
        CurrentSwipeDirection = SwipeDirection.None;
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