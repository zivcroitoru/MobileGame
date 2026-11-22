using UnityEngine;

[DisallowMultipleComponent]
public class CatRagdoll : MonoBehaviour
{
    [SerializeField] private Rigidbody _pelvis;

    private Collider[] _colliders;

    public Rigidbody Pelvis => _pelvis;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>();
        foreach (var body in GetComponentsInChildren<Rigidbody>())
        {
            body.solverIterations = 20;
            body.solverVelocityIterations = 8;
            body.maxDepenetrationVelocity = 2.5f;
            body.maxAngularVelocity = 12f;
        }
    }

    private void OnEnable()
    {
        // Tight curls can overlap distant limbs. Keep those contacts out of the joint solver.
        for (int first = 0; first < _colliders.Length; first++)
        {
            for (int second = first + 1; second < _colliders.Length; second++)
                Physics.IgnoreCollision(_colliders[first], _colliders[second]);
        }
    }
}
