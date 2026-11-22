using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    [SerializeField] private Rigidbody _catPelvis;
    [SerializeField] private LevelResultController _result;

    public Rigidbody CatPelvis => _catPelvis;
    public LevelResultController Result => _result;

    private void OnTriggerEnter(Collider other)
    {
        if (_catPelvis != null && other.attachedRigidbody == _catPelvis)
            _result.Win();
    }
}
