using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CatRagdoll))]
public class TapToPush3D : MonoBehaviour
{
    [SerializeField, Min(0f), Tooltip("Horizontal impulse in newton-seconds.")]
    private float _horizontalImpulse = 15f;
    [SerializeField, Min(0f), Tooltip("Upward impulse in newton-seconds.")]
    private float _upwardImpulse = 40f;
    [SerializeField, Min(0.01f), Tooltip("Horizontal ragdoll speed limit for launch impulses, in metres per second.")]
    private float _maxSpeed = 5.5f;
    [SerializeField, Min(0f)] private float _tapCooldown = 0.26f;

    [SerializeField, Min(0.01f)] private float _maxUpwardSpeed = 6.8f;
    [SerializeField, Min(0f), Tooltip("Additional roll speed per boost, in radians per second.")]
    private float _launchSpin = 1.6f;

    private readonly List<RaycastResult> _uiHits = new();
    private PointerEventData _pointer;
    private CatRagdoll _ragdoll;
    private double _nextPushTime;
    private float _queuedDirection;

    public float LaunchDirection { get; private set; } = 1f;

    private void Awake()
    {
        _ragdoll = GetComponent<CatRagdoll>();

        if (_ragdoll.Pelvis == null)
        {
            Debug.LogError("Tap To Push needs a pelvis Rigidbody on the ragdoll.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        _queuedDirection = 0f;
    }

    private void Update()
    {
        var touch = Touchscreen.current?.primaryTouch;
        if (touch != null && touch.press.wasPressedThisFrame)
        {
            QueuePush(touch.position.ReadValue());
        }
        else if (Mouse.current is { } mouse && mouse.leftButton.wasPressedThisFrame)
        {
            QueuePush(mouse.position.ReadValue());
        }
    }

    private void QueuePush(Vector2 position)
    {
        if (_queuedDirection != 0f || Time.timeAsDouble < _nextPushTime)
            return;

        var screen = new Rect(0f, 0f, Screen.width, Screen.height);
        if (screen.Contains(position) && !IsOverInteractiveUI(position))
            _queuedDirection = position.x < screen.center.x ? -1f : 1f;
    }

    private bool IsOverInteractiveUI(Vector2 position)
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        // Raycast this press directly, independently of when the UI input module updates.
        _pointer ??= new PointerEventData(eventSystem);
        _pointer.position = position;
        eventSystem.RaycastAll(_pointer, _uiHits);
        foreach (var hit in _uiHits)
        {
            if (hit.module is GraphicRaycaster &&
                (ExecuteEvents.GetEventHandler<IPointerDownHandler>(hit.gameObject) != null ||
                 ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit.gameObject) != null))
                return true;
        }

        return false;
    }

    private void FixedUpdate()
    {
        if (_queuedDirection == 0f)
            return;

        LaunchDirection = _queuedDirection;
        _queuedDirection = 0f;
        _nextPushTime = Time.timeAsDouble + _tapCooldown;
        _ragdoll.Launch(LaunchDirection, _horizontalImpulse, _upwardImpulse, _maxSpeed, _maxUpwardSpeed, _launchSpin);
    }
}
