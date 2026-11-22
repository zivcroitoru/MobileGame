
using UnityEngine;

[DisallowMultipleComponent]
public class CatRagdoll : MonoBehaviour
{
    [SerializeField] private Rigidbody _pelvis;

    [Header("Stability")]
    [SerializeField, Range(1f, 10f)]
    private float _maxSpinSpeed = 6f;

    [SerializeField, Range(0.1f, 1f)]
    private float _spinMultiplier = 0.65f;

    private Collider[] _colliders;
    private Rigidbody[] _bodies;
    private float _mass;

    public Rigidbody Pelvis => _pelvis;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>(true);
        _bodies = GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody body in _bodies)
        {
            _mass += body.mass;

            // Smooth physics between rendered frames.
            body.interpolation =
                RigidbodyInterpolation.Interpolate;

            // Improve joint stability.
            body.solverIterations = 20;
            body.solverVelocityIterations = 12;

            // Prevent excessive angular motion.
            body.maxAngularVelocity = 10f;
        }
    }

    public void Launch(
        float direction,
        float horizontalImpulse,
        float upwardImpulse,
        float maxSpeed,
        float maxUpwardSpeed,
        float launchSpin)
    {
        if (_mass <= 0f)
            return;

        direction = direction >= 0f ? 1f : -1f;

        Vector3 momentum = Vector3.zero;
        Vector3 weightedCenter = Vector3.zero;

        foreach (Rigidbody body in _bodies)
        {
            momentum += body.linearVelocity * body.mass;
            weightedCenter +=
                body.worldCenterOfMass * body.mass;
        }

        Vector3 velocity = momentum / _mass;
        Vector3 center = weightedCenter / _mass;

        float horizontal = Mathf.Min(
            horizontalImpulse / _mass,
            Mathf.Max(0f, maxSpeed - velocity.x * direction)
        );

        float vertical = Mathf.Min(
            upwardImpulse / _mass,
            Mathf.Max(0f, maxUpwardSpeed - velocity.y)
        );

        Vector3 launchVelocity = new Vector3(
            direction * horizontal,
            vertical,
            0f
        );

        // Limit additional rotation during each launch.
        float currentSpin = _pelvis != null
            ? -_pelvis.angularVelocity.z * direction
            : 0f;

        float spinHeadroom =
            Mathf.Max(0f, _maxSpinSpeed - currentSpin);

        float spinAmount = Mathf.Min(
            Mathf.Max(0f, launchSpin) * _spinMultiplier,
            spinHeadroom
        );

        Vector3 roll =
            Vector3.back * (direction * spinAmount);

        foreach (Rigidbody body in _bodies)
        {
            body.WakeUp();

            // Apply consistent motion across the ragdoll.
            Vector3 rollVelocity = Vector3.Cross(
                roll,
                body.worldCenterOfMass - center
            );

            body.AddForce(
                launchVelocity + rollVelocity,
                ForceMode.VelocityChange
            );

            body.AddTorque(
                roll,
                ForceMode.VelocityChange
            );
        }
    }

    private void OnEnable()
    {
        if (_colliders == null)
            return;

        // Disable ragdoll self-collisions.
        for (int i = 0; i < _colliders.Length; i++)
        {
            for (int j = i + 1; j < _colliders.Length; j++)
            {
                Physics.IgnoreCollision(
                    _colliders[i],
                    _colliders[j],
                    true
                );
            }
        }
    }
}
