using UnityEngine;

/// <summary>
/// Keeps decorative scenery moving more slowly than the side-scrolling camera.
/// No physics, colliders or gameplay dependencies.
/// </summary>
[DisallowMultipleComponent]
public sealed class PawPathParallax : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField, Range(0f, 1f)] private float horizontalFollow = 0.75f;
    [SerializeField, Range(0f, 1f)] private float verticalFollow = 0.45f;

    private Vector3 initialPosition;
    private Vector3 initialCameraPosition;

    public void Configure(Transform target, float horizontal, float vertical)
    {
        cameraTarget = target;
        horizontalFollow = Mathf.Clamp01(horizontal);
        verticalFollow = Mathf.Clamp01(vertical);
    }

    private void Start()
    {
        if (cameraTarget == null && Camera.main != null)
            cameraTarget = Camera.main.transform;

        initialPosition = transform.position;
        if (cameraTarget != null)
            initialCameraPosition = cameraTarget.position;
    }

    private void LateUpdate()
    {
        if (cameraTarget == null) return;

        Vector3 delta = cameraTarget.position - initialCameraPosition;
        transform.position = initialPosition + new Vector3(
            delta.x * horizontalFollow,
            delta.y * verticalFollow,
            0f);
    }
}
