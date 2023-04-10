using PathCreation;
using Unity.Netcode;
using UnityEngine;


public abstract class Follower : NetworkBehaviour
{
    protected static VertexPath _mainPath => MainPath.Path;

    [SerializeField] protected FollowerParameters _parameters;
    protected float _distanceTravelled = 0f;

    protected abstract float Speed { get; set; }
    protected abstract float Offset { get; set; }

    protected float GetRandomSpeed()
    {
        var randomSpeed = Random.Range(
            _parameters.MinSpeed,
            _parameters.MaxSpeed);
        return randomSpeed;
    }

    protected bool MatchesWithPathDirection(Vector3 direction)
    {
        var pathDirection = GetPathDirection();
        var angle = Vector3.Angle(pathDirection, direction);
        return angle < 90f;
    }

    private Vector3 GetPathDirection()
    {
        return _mainPath.GetDirectionAtDistance(this._distanceTravelled);
    }

    protected void FixedUpdate()
    {
        UpdateDistanceTravelled();

        transform.rotation = GetRotation();
        transform.position = GetPosition();
    }

    private void UpdateDistanceTravelled()
    {
        this._distanceTravelled = Mathf.Repeat(this._distanceTravelled + this.Speed, _mainPath.length);
    }

    private Vector3 GetPosition()
    {
        var positionAlongPath = GetPointAtDistance();
        var offsetVector = GetOffsetVector();

        return positionAlongPath + offsetVector + Vector3.up;
    }

    protected Vector3 GetPointAtDistance()
    {
        return _mainPath.GetPointAtDistance(this._distanceTravelled);
    }

    private Vector3 GetOffsetVector()
    {
        var offsetVector = this.Offset * transform.right;
        if (IsReversed())
            offsetVector *= -1;
        return offsetVector;
    }

    protected Quaternion GetRotation()
    {
        var rotation = _mainPath.GetRotationAtDistance(this._distanceTravelled) * _parameters.RotationOffset;
        if (IsReversed())
            rotation = GetOppositeRotation(rotation);
        return rotation;
    }

    protected bool IsReversed()
    {
        return this.Speed < 0f;
    }

    private Quaternion GetOppositeRotation(Quaternion rotation)
    {
        return _parameters.ReversingRotation * rotation;
    }
}
