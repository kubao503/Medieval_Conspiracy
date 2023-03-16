using PathCreation;
using Unity.Netcode;
using UnityEngine;


public abstract class Follower : NetworkBehaviour
{
    private static PathCreator _mainPath;
    private const float _minSpeed = 0.01f;
    private const float _maxSpeed = 0.03f;

    protected float _distanceTravelled = 0f;
    protected readonly Quaternion _rotationOffset = Quaternion.Euler(0f, 0f, 90f);
    private readonly Quaternion _reversingRotation = Quaternion.Euler(0f, 180f, 0f);

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
        var randomSpeed = Random.Range(_minSpeed, _maxSpeed);
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

    private void FixedUpdate()
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
        return this.Offset * transform.right;
    }

    protected Quaternion GetRotation()
    {
        var rotation = MainPath.path.GetRotationAtDistance(this._distanceTravelled) * this._rotationOffset;
        if (IsReversed())
            rotation = GetOppositeRotation(rotation);
        return rotation;
    }

    private bool IsReversed()
    {
        return this.Speed < 0f;
    }

    private Quaternion GetOppositeRotation(Quaternion rotation)
    {
        return this._reversingRotation * rotation;
    }
}
