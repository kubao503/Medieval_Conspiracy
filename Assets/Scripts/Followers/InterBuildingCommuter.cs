using UnityEngine;


public class InterBuildingCommuter : MonoBehaviour
{
    private State _state = State.OnPath;
    private Vector3 _leavingTargetOnPath;
    private Vector3 _buildingTarget;
    private float _velocity = .3f;
    private const float _delta = .1f;

    enum State
    {
        Leaving,
        OnPath,
        Entering
    }

    private void Awake()
    {
        _leavingTargetOnPath = MainPath.Path.GetClosestPointOnPath(transform.position);
        _buildingTarget = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (_state == State.Leaving)
            Leave();
    }

    private void Leave()
    {
        var direction = _leavingTargetOnPath - transform.position;
        if (direction.sqrMagnitude <= _delta * _delta)
            _state = State.OnPath;

        transform.position += _velocity * direction.normalized;
    }
}
