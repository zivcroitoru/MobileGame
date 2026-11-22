using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class SideScrollCamera : MonoBehaviour
{
    [SerializeField] private Rigidbody _target;
    [SerializeField, Range(0.05f, 0.5f)] private float _targetViewportX = 0.3f;
    [SerializeField, Min(0.01f)] private float _followDamping = 0.12f;

    private Camera _camera;
    private float _followVelocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        if (_target != null)
            SetCameraX(GetTargetX());
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        float x = Mathf.SmoothDamp(transform.position.x, GetTargetX(), ref _followVelocity, _followDamping);
        SetCameraX(x);
    }

    private float GetTargetX()
    {
        float viewWidth = 2f * _camera.orthographicSize * _camera.aspect;
        return _target.worldCenterOfMass.x + (0.5f - _targetViewportX) * viewWidth;
    }

    private void SetCameraX(float x)
    {
        Vector3 position = transform.position;
        position.x = x;
        transform.position = position;
    }
}
