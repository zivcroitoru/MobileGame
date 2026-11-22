using UnityEngine;

[DisallowMultipleComponent]
public class CatRagdoll : MonoBehaviour
{
    [Header("Ragdoll")]
    [SerializeField] private Rigidbody _pelvis;

    [Header("Launch stability")]
    [SerializeField, Range(0f, 1f)] private float _spinMultiplier = 0.65f;
    [SerializeField, Min(0f)] private float _maxSpinSpeed = 6f;

    private Collider[] _colliders;
    private Rigidbody[] _bodies;
    private float _totalMass;

    public Rigidbody Pelvis => _pelvis;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>(true);
        _bodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody body in _bodies)
        {
            _totalMass += body.mass;

            // Interpolate each independently simulated ragdoll part.
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.solverIterations = 20;
            body.solverVelocityIterations = 12;
            body.maxDepenetrationVelocity = 2.5f;
            body.maxAngularVelocity = 12f;
        }

        // OnEnable can run before Awake: ensure self-collisions are
        // disabled immediately after the colliders are collected.
        IgnoreSelfCollisions();
    }

    private void OnEnable()
    {
        if (_colliders != null)
            IgnoreSelfCollisions();
    }

    private void IgnoreSelfCollisions()
    {
        // The joints, not collider contacts, hold the ragdoll together.
        for (int i = 0; i < _colliders.Length; i++)
        {
            for (int j = i + 1; j < _colliders.Length; j++)
                Physics.IgnoreCollision(_colliders[i], _colliders[j], true);
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
        if (_totalMass <= 0f || _pelvis == null)
            return;

        direction = direction >= 0f ? 1f : -1f;

        // Use the entire ragdoll's momentum, not one bone's speed.
        Vector3 momentum = Vector3.zero;
        foreach (Rigidbody body in _bodies)
            momentum += body.linearVelocity * body.mass;

        Vector3 averageVelocity = momentum / _totalMass;

        float horizontalDelta = Mathf.Min(
            Mathf.Max(0f, horizontalImpulse) / _totalMass,
            Mathf.Max(0f, maxSpeed - averageVelocity.x * direction));

        float upwardDelta = Mathf.Min(
            Mathf.Max(0f, upwardImpulse) / _totalMass,
            Mathf.Max(0f, maxUpwardSpeed - averageVelocity.y));

        Vector3 velocityChange = new Vector3(
            direction * horizontalDelta, upwardDelta, 0f);

        // Give every body the SAME velocity change: less strain on joints.
        foreach (Rigidbody body in _bodies)
        {
            if (body.isKinematic)
                continue;

            body.WakeUp();
            body.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        // Spin the pelvis only. Applying torque to every limb makes joints fight.
        if (!_pelvis.isKinematic && launchSpin > 0f)
        {
            float currentSpin = -_pelvis.angularVelocity.z * direction;
            float availableSpin = Mathf.Max(0f, _maxSpinSpeed - currentSpin);
            float addedSpin = Mathf.Min(launchSpin * _spinMultiplier, availableSpin);

            _pelvis.AddTorque(
                Vector3.back * (direction * addedSpin),
                ForceMode.VelocityChange);
        }
    }
}
