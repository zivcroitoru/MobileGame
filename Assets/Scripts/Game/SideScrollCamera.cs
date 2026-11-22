
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class SideScrollCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Rigidbody _target;
    [SerializeField] private TapToPush3D _launchControls;

    [Header("Framing")]
    [SerializeField, Range(0.05f, 0.5f)]
    private float _targetViewportX = 0.3f;

    [SerializeField, Range(-0.5f, 0.5f)]
    private float _verticalOffset = 0.18f;

    [Header("Smooth Following")]
    [SerializeField, Range(0.02f, 0.5f)]
    private float _followSmoothTime = 0.10f;

    [SerializeField, Range(0.02f, 1f)]
    private float _directionSmoothTime = 0.25f;

    private Camera _camera;
    private Vector2 _followVelocity;

    private float _currentLookAhead;
    private float _lookAheadVelocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        if (_target == null)
            return;

        // Interpolate physics movement for smooth rendering.
        _target.interpolation = RigidbodyInterpolation.Interpolate;

        float direction = GetLaunchDirection();
        _currentLookAhead = GetLookAhead(direction);

        // Start at the correct position without an initial jump.
        Vector2 initialPosition = GetTargetPosition();
        transform.position = new Vector3(
            initialPosition.x,
            initialPosition.y,
            transform.position.z
        );
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        // Smooth direction changes instead of instantly
        // jumping to the other side of the cat.
        float desiredLookAhead =
            GetLookAhead(GetLaunchDirection());

        _currentLookAhead = Mathf.SmoothDamp(
            _currentLookAhead,
            desiredLookAhead,
            ref _lookAheadVelocity,
            _directionSmoothTime,
            Mathf.Infinity,
            dt
        );

        Vector2 targetPosition = GetTargetPosition();

        // Smooth camera movement with minimal lag.
        Vector2 newPosition = Vector2.SmoothDamp(
            new Vector2(transform.position.x, transform.position.y),
            targetPosition,
            ref _followVelocity,
            _followSmoothTime,
            Mathf.Infinity,
            dt
        );

        // Preserve camera depth.
        transform.position = new Vector3(
            newPosition.x,
            newPosition.y,
            transform.position.z
        );
    }

    private float GetLaunchDirection()
    {
        return _launchControls != null
            ? _launchControls.LaunchDirection
            : 1f;
    }

    private float GetLookAhead(float direction)
    {
        float viewWidth =
            2f * _camera.orthographicSize * _camera.aspect;

        return direction *
            (0.5f - _targetViewportX) * viewWidth;
    }

    private Vector2 GetTargetPosition()
    {
        // Use the interpolated Transform position instead
        // of sampling the physics center of mass directly.
        Vector3 position = _target.transform.position;

        return new Vector2(
            position.x + _currentLookAhead,
            position.y +
                _camera.orthographicSize * _verticalOffset
        );
    }
}
