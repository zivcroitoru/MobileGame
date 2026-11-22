using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CatRagdoll))]
public class TapToMove3D : MonoBehaviour
{
    private const float VelocityResponseTime = 0.15f;
    private const float SettledSpeed = 0.12f;

    [SerializeField] private Camera _gameplayCamera;
    [SerializeField] private Rigidbody _driveBody;
    [SerializeField] private float _groundHeight;
    [SerializeField, Tooltip("World-space X/Z bounds for tap targets, inset from the arena walls.")]
    private Rect _playableArea = new(-1.25f, -2.85f, 2.5f, 5.7f);

    [SerializeField, Min(0.01f)] private float _moveSpeed = 2.1f;
    [SerializeField, Min(0.01f)] private float _acceleration = 7.5f;
    [SerializeField, Min(0.01f)] private float _stoppingDistance = 0.14f;
    [SerializeField, Min(0.01f)] private float _maxForce = 38f;

    private readonly List<RaycastResult> _uiHits = new();
    private PointerEventData _pointer;
    private CatRagdoll _ragdoll;
    private Vector3 _target;
    private bool _hasTarget;

    private void Awake()
    {
        _ragdoll = GetComponent<CatRagdoll>();

        if (_gameplayCamera == null || _driveBody == null)
        {
            Debug.LogError("Tap To Move needs the gameplay camera and the pelvis Rigidbody.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        _hasTarget = false;
    }

    private void Update()
    {
        var touch = Touchscreen.current?.primaryTouch;
        if (touch != null && touch.press.wasPressedThisFrame)
        {
            SetTargetFromScreen(touch.position.ReadValue());
        }
        else if (Mouse.current is { } mouse && mouse.leftButton.wasPressedThisFrame)
        {
            SetTargetFromScreen(mouse.position.ReadValue());
        }
    }

    private void SetTargetFromScreen(Vector2 position)
    {
        if (!_gameplayCamera.pixelRect.Contains(position) || IsOverInteractiveUI(position))
            return;

        var floor = new Plane(Vector3.up, new Vector3(0f, _groundHeight, 0f));
        Ray ray = _gameplayCamera.ScreenPointToRay(position);
        if (!floor.Raycast(ray, out float distance))
            return;

        Vector3 point = ray.GetPoint(distance);
        _target = new Vector3(
            Mathf.Clamp(point.x, _playableArea.xMin, _playableArea.xMax),
            _groundHeight,
            Mathf.Clamp(point.z, _playableArea.yMin, _playableArea.yMax));
        _hasTarget = true;
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
        if (!_hasTarget)
            return;

        Vector3 offset = _target - _driveBody.worldCenterOfMass;
        offset.y = 0f;
        Vector3 velocity = _driveBody.linearVelocity;
        velocity.y = 0f;

        float distance = offset.magnitude;
        bool arrived = distance <= _stoppingDistance;
        if (arrived && velocity.sqrMagnitude <= SettledSpeed * SettledSpeed)
        {
            _hasTarget = false;
            return;
        }

        // One force at the pelvis' center of mass pulls the rest of the rig through its joints.
        float mass = _ragdoll.TotalMass;
        float forceLimit = Mathf.Min(_maxForce, _acceleration * mass);
        float brakingDistance = Mathf.Max(0f, distance - _stoppingDistance * 0.5f);
        float brakingSpeed = Mathf.Sqrt(2f * forceLimit / mass * brakingDistance);
        float desiredSpeed = arrived ? 0f : Mathf.Min(_moveSpeed, brakingSpeed);
        Vector3 desiredVelocity = offset.normalized * desiredSpeed;
        Vector3 force = (desiredVelocity - velocity) * (mass / VelocityResponseTime);
        _driveBody.AddForce(Vector3.ClampMagnitude(force, forceLimit));
    }
}
