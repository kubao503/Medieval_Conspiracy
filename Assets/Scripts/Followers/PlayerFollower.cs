using UnityEngine;


public class PlayerFollower : Follower
{
    private float _speed;
    private float _offset;

    protected override float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    protected override float Offset
    {
        get => _offset;
        set => _offset = value;
    }

    private void OnEnable()
    {
        // Order of following calls must be preserved
        SetInitDistance();
        SetInitSpeed();
        SetInitRotation();
        SetInitOffset();
    }

    private void SetInitDistance()
    {
        this._distanceTravelled = MainPath.path.GetClosestDistanceAlongPath(transform.position);
    }

    private void SetInitSpeed()
    {
        this.Speed = GetRandomSpeed();

        if (!MatchesWithPathDirection(transform.forward))
            RevertPathFollowingDirection();
    }

    private void RevertPathFollowingDirection()
    {
        this.Speed *= -1;
    }

    private void SetInitRotation()
    {
        transform.rotation = GetRotation();
    }

    private void SetInitOffset()
    {
        var closestPoint = GetPointAtDistance();
        var offsetVector = transform.position - closestPoint;
        var localOffsetVector = transform.InverseTransformDirection(offsetVector);
        this.Offset = localOffsetVector.x;
    }
}
