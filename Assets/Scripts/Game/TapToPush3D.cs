using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Save as TapToPush3D.cs (the file name must match the MonoBehaviour).
[DisallowMultipleComponent]
[RequireComponent(typeof(CatRagdoll))]
public class TapToPush3D : MonoBehaviour
{
    [Header("Launch force (N·s)")]
    [SerializeField, Min(0f)] private float _horizontalImpulse = 15f;
    [SerializeField, Min(0f)] private float _upwardImpulse = 40f;
    [SerializeField, Min(0.1f)] private float _maxSpeed = 8f;
    [SerializeField, Min(0.1f)] private float _maxUpwardSpeed = 10f;
    [SerializeField, Min(0f)] private float _launchSpin = 1.6f;
    [SerializeField, Min(0f)] private float _tapCooldown = 0.26f;

    [Header("Swipe control")]
    [SerializeField, Range(0.01f, 0.15f)] private float _minimumSwipeScreenHeight = 0.035f;
    [SerializeField, Range(0.05f, 0.6f)] private float _fullPowerSwipeScreenHeight = 0.25f;
    [SerializeField, Range(0.1f, 1f)] private float _minimumSwipePower = 0.75f;
    [SerializeField, Range(1f, 2f)] private float _maximumSwipePower = 1.5f;
    [SerializeField, Range(0f, 0.5f)] private float _upSwipeBonus = 0.25f;
    [SerializeField] private bool _allowTap = true;

    private readonly List<RaycastResult> _uiHits = new();
    private PointerEventData _pointer;
    private CatRagdoll _ragdoll;
    private bool _trackingPress;
    private bool _launchQueued;
    private Vector2 _pressStart;
    private float _queuedDirection;
    private float _queuedPower;
    private float _queuedUpwardMultiplier;
    private double _nextPushTime;

    public float LaunchDirection { get; private set; } = 1f;

    private void Awake()
    {
        _ragdoll = GetComponent<CatRagdoll>();
        if (_ragdoll.Pelvis == null)
        {
            Debug.LogError("TapToPush3D needs a pelvis Rigidbody on CatRagdoll.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        _trackingPress = false;
        _launchQueued = false;
    }

    private void Update()
    {
        // Read touch and mouse in Update, then apply physics in FixedUpdate.
        var touch = Touchscreen.current?.primaryTouch;
        if (touch != null && (touch.press.isPressed ||
                              touch.press.wasPressedThisFrame ||
                              touch.press.wasReleasedThisFrame))
        {
            Vector2 position = touch.position.ReadValue();
            if (touch.press.wasPressedThisFrame) BeginGesture(position);
            if (touch.press.wasReleasedThisFrame) EndGesture(position);
            return;
        }

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePosition = mouse.position.ReadValue();
        if (mouse.leftButton.wasPressedThisFrame) BeginGesture(mousePosition);
        if (mouse.leftButton.wasReleasedThisFrame) EndGesture(mousePosition);
    }

    private void BeginGesture(Vector2 position)
    {
        _trackingPress = false;
        if (_launchQueued || Time.timeAsDouble < _nextPushTime ||
            !IsValidScreenPoint(position) || IsOverInteractiveUI(position))
            return;

        _pressStart = position;
        _trackingPress = true;
    }

    private void EndGesture(Vector2 position)
    {
        if (!_trackingPress) return;
        _trackingPress = false;

        if (_launchQueued || Time.timeAsDouble < _nextPushTime ||
            !IsValidScreenPoint(position))
            return;

        Vector2 drag = position - _pressStart;
        float screenHeight = Mathf.Max(1f, Screen.height);
        float swipeFraction = drag.magnitude / screenHeight;
        bool isSwipe = swipeFraction >= _minimumSwipeScreenHeight;

        if (!isSwipe && !_allowTap) return;

        // Choose direction from the swipe's horizontal movement.
        // Nearly vertical gestures and taps use the starting screen half.
        float minimumHorizontalDistance = _minimumSwipeScreenHeight * screenHeight;
        _queuedDirection = isSwipe && Mathf.Abs(drag.x) >= minimumHorizontalDistance
            ? Mathf.Sign(drag.x)
            : (_pressStart.x < Screen.width * 0.5f ? -1f : 1f);

        // A longer swipe delivers more impulse, up to a configurable limit.
        float swipeT = Mathf.InverseLerp(
            _minimumSwipeScreenHeight,
            Mathf.Max(_minimumSwipeScreenHeight + 0.001f, _fullPowerSwipeScreenHeight),
            swipeFraction);
        _queuedPower = isSwipe
            ? Mathf.Lerp(_minimumSwipePower, _maximumSwipePower, swipeT)
            : 1f;

        // Every gesture lifts the cat; upward swipes add a small extra lift.
        float upwardT = Mathf.Clamp01(drag.y / (screenHeight * 0.25f));
        _queuedUpwardMultiplier = 1f + upwardT * _upSwipeBonus;
        _launchQueued = true;
    }

    private void FixedUpdate()
    {
        if (!_launchQueued) return;
        _launchQueued = false;

        LaunchDirection = _queuedDirection;
        _nextPushTime = Time.timeAsDouble + _tapCooldown;

        // CatRagdoll.Launch applies forces only in the XY side-scrolling plane.
        _ragdoll.Launch(
            LaunchDirection,
            _horizontalImpulse * _queuedPower,
            _upwardImpulse * _queuedPower * _queuedUpwardMultiplier,
            _maxSpeed,
            _maxUpwardSpeed,
            _launchSpin * _queuedPower);
    }

    private static bool IsValidScreenPoint(Vector2 position)
    {
        return position.x >= 0f && position.x <= Screen.width &&
               position.y >= 0f && position.y <= Screen.height;
    }

    private bool IsOverInteractiveUI(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        _pointer ??= new PointerEventData(eventSystem);
        _pointer.Reset();
        _pointer.position = position;
        _uiHits.Clear();
        eventSystem.RaycastAll(_pointer, _uiHits);

        foreach (RaycastResult hit in _uiHits)
        {
            if (hit.module is GraphicRaycaster &&
                (ExecuteEvents.GetEventHandler<IPointerDownHandler>(hit.gameObject) != null ||
                 ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit.gameObject) != null))
                return true;
        }
        return false;
    }
}
