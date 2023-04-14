using UnityEngine;
using Unity.Netcode;


public class ResidentFollower : Follower
{
    private readonly NetworkVariable<float> _netSpeed = new();
    private readonly NetworkVariable<float> _netOffset = new();
    private float _defaultSpeed = 1f;

    protected override float Speed
    {
        set => this._netSpeed.Value = value;
        get => this._netSpeed.Value;
    }

    protected override float Offset
    {
        set => this._netOffset.Value = value;
        get => this._netOffset.Value;
    }

    public void SetRandomPositionAlongPath()
    {
        SetRandomDistance();
        SetRandomOffset();
    }

    private void SetRandomDistance()
    {
        var randomDistance = Random.Range(0f, GetPathLength());
        this._distanceTravelled = randomDistance;
    }

    private float GetPathLength()
    {
        return _mainPath.length;
    }

    private void SetRandomOffset()
    {
        var randomOffset = Random.Range(
            -_parameters.OffsetRange, _parameters.OffsetRange);
        this.Offset = randomOffset;
    }

    public void SetSpeedToDefault()
    {
        SetSpeedPreservingDirection(_defaultSpeed);
    }

    private void SetSpeedPreservingDirection(float speed)
    {
        this.Speed = speed * Mathf.Sign(this.Speed);
    }

    public void RunAwayFromDanger(Vector3 dangerPosition)
    {
        var directionResidentToDanger = dangerPosition - transform.position;

        var newSpeed = _parameters.RunSpeed;
        if (MatchesWithPathDirection(directionResidentToDanger))
            newSpeed *= -1;
        this.Speed = newSpeed;
    }

    public void DistanceSync()
    {
        DistanceSyncClientRpc(this._distanceTravelled);
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void DistanceSyncClientRpc(float distance)
    {
        if (!IsServer)
            this._distanceTravelled = distance;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SetInitSpeed();
            SetRandomPositionAlongPath();
        }
        base.OnNetworkSpawn();
    }

    private void SetInitSpeed()
    {
        var randomSpeed = GetRandomSpeed();

        this.Speed = randomSpeed;
        this._defaultSpeed = randomSpeed;

        SetRandomDiretion();
    }

    private void SetRandomDiretion()
    {
        if (Random.value < .5)
            this.Speed *= -1;
    }
}
