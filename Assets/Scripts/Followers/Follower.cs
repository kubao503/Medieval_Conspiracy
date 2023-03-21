using PathCreation;
using Unity.Netcode;
using UnityEngine;


public abstract class Follower : NetworkBehaviour
{
    private static PathCreator _mainPath;

    [SerializeField] protected FollowerParameters _parameters;
    protected float _distanceTravelled = 0f;

    protected abstract float Speed { get; set; }
    protected abstract float Offset { get; set; }

    // TODO: Make protected
    public static PathCreator MainPath
    {
        get
        {
            SetMainPath();
            return _mainPath;
        }
    }

    private static void SetMainPath()
    {
        if (_mainPath == null)
            _mainPath = GameObject.Find("NPC Path").GetComponent<PathCreator>();
    }

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
        return MainPath.path.GetDirectionAtDistance(this._distanceTravelled);
    }

    protected void FixedUpdate()
    {
        UpdateDistanceTravelled();

        var position = GetPosition();
        var rotation = GetRotation();

        transform.SetPositionAndRotation(position, rotation);
    }

    private void UpdateDistanceTravelled()
    {
        this._distanceTravelled = Mathf.Repeat(this._distanceTravelled + this.Speed, MainPath.path.length);
    }

    private Vector3 GetPosition()
    {
        var positionAlongPath = GetPointAtDistance();
        var offsetVector = GetOffsetVector();

        return positionAlongPath + offsetVector + Vector3.up;
    }

    protected Vector3 GetPointAtDistance()
    {
        return MainPath.path.GetPointAtDistance(this._distanceTravelled);
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
        var rotation = MainPath.path.GetRotationAtDistance(this._distanceTravelled) * _parameters.RotationOffset;
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
