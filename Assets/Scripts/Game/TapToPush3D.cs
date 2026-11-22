using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CatRagdoll))]
public class TapToPush3D : MonoBehaviour
{
    [SerializeField, Min(0f), Tooltip("Rightward impulse in newton-seconds.")]
    private float _horizontalImpulse = 11f;
    [SerializeField, Min(0f), Tooltip("Upward impulse in newton-seconds.")]
    private float _upwardImpulse = 4.8f;
    [SerializeField, Min(0.01f), Tooltip("Rightward pelvis speed limit for tap impulses, in metres per second.")]
    private float _maxSpeed = 8.5f;
    [SerializeField, Min(0f)] private float _tapCooldown = 0.22f;

    private readonly List<RaycastResult> _uiHits = new();
    private PointerEventData _pointer;
    private Rigidbody _pelvis;
    private double _nextPushTime;
    private bool _pushQueued;

    private void Awake()
    {
        _pelvis = GetComponent<CatRagdoll>().Pelvis;

        if (_pelvis == null)
        {
            Debug.LogError("Tap To Push needs a pelvis Rigidbody on the ragdoll.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        _pushQueued = false;
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
        if (_pushQueued || Time.timeAsDouble < _nextPushTime)
            return;

        var screen = new Rect(0f, 0f, Screen.width, Screen.height);
        if (screen.Contains(position) && !IsOverInteractiveUI(position))
            _pushQueued = true;
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
        if (!_pushQueued)
            return;

        _pushQueued = false;
        _nextPushTime = Time.timeAsDouble + _tapCooldown;

        // Cap the immediate velocity change using the mass that receives the impulse.
        float speedHeadroom = Mathf.Max(0f, _maxSpeed - _pelvis.linearVelocity.x);
        float horizontal = Mathf.Min(_horizontalImpulse, _pelvis.mass * speedHeadroom);
        _pelvis.AddForce(new Vector3(horizontal, _upwardImpulse, 0f), ForceMode.Impulse);
    }
}
