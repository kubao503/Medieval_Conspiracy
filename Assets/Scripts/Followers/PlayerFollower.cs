using UnityEngine;


public class PlayerFollower : Follower
{
    private float _speed;
    private float _offset;
    private IInput _input = InputAdapter.Instance;
    private PlayerState _playerState;
    private const KeyCode _pathFollowingKey = KeyCode.Q;

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

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            _playerState.StateUpdated += StateUpdated;
        else
            this.enabled = false;
        base.OnNetworkSpawn();
    }

    private void StateUpdated(object sender, StateEventArgs args)
    {
        if (args.NewState == PlayerState.State.OnPath)
            StartPathFollowing();
    }

    private void StartPathFollowing()
    {
        // Order of following calls must be preserved
        SetInitDistance();
        SetInitSpeed();
        SetInitRotation();
        SetInitOffset();
    }

    private void SetInitDistance()
    {
        this._distanceTravelled = _mainPath.GetClosestDistanceAlongPath(transform.position);
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

    private void Update()
    {
        if (_input.GetKeyDown(_pathFollowingKey))
            TogglePathFollowing();
    }

    private void TogglePathFollowing()
    {
        if (IsCloseEnoughToPath())
            _playerState.TogglePathFollowingStateServerRpc();
    }

    private bool IsCloseEnoughToPath()
    {
        var closestPointOnPath = _mainPath.GetClosestPointOnPath(transform.position);
        var distance = GetDistance2D(closestPointOnPath, transform.position);
        return distance <= _parameters.OffsetRange;
    }

    private float GetDistance2D(Vector3 first, Vector3 second)
    {
        Vector2 first2D = new(first.x, first.z);
        Vector2 second2D = new(second.x, second.z);
        return Vector2.Distance(first2D, second2D);
    }

    private new void FixedUpdate()
    {
        if (_playerState.CurrentState == PlayerState.State.OnPath)
            base.FixedUpdate();
    }
}
