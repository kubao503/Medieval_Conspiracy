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

        if (IsTooFarFromPath())
        {
            this.enabled = false;
            return;
        }

        SetInitSpeed();
        SetInitRotation();
        SetInitOffset();
    }

    private void SetInitDistance()
    {
        this._distanceTravelled = MainPath.path.GetClosestDistanceAlongPath(transform.position);
    }

    private bool IsTooFarFromPath()
    {
        var closestPointOnPath = MainPath.path.GetPointAtDistance(_distanceTravelled);
        var distance = GetDistance2D(closestPointOnPath, transform.position);
        return distance > _parameters.OffsetRange;
    }

    private float GetDistance2D(Vector3 first, Vector3 second)
    {
        Vector2 first2D = new(first.x, first.z);
        Vector2 second2D = new(second.x, second.z);
        return Vector2.Distance(first2D, second2D);
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
        var offset = GetInitOffset();
        if (IsReversed())
            offset *= -1;
        this.Offset = offset;
    }

    private float GetInitOffset()
    {
        var closestPoint = GetPointAtDistance();
        var offsetVector = transform.position - closestPoint;
        var localOffsetVector = transform.InverseTransformDirection(offsetVector);
        return localOffsetVector.x;
    }
}
