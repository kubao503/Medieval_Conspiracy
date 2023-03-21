using UnityEngine;
using Unity.Netcode;


public class PlayerAttacker : BaseAttacker
{
    private IInput _input = InputAdapter.Instance;
    private PlayerState _playerState;
    private PlayerHostility _playerHostility;
    private TeamController _teamController;

    private new void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _playerHostility = GetComponent<PlayerHostility>();
        _teamController = GetComponent<TeamController>();
        base.Awake();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            this.enabled = false;
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        var stateOutside = _playerState.CurrentState == PlayerState.State.Outside;
        if (_input.GetLeftMouseButtonDown() && stateOutside)
        {
            HitNearbyTargetsServerRpc();
            _animator.PlayHitAnimation();
        }
    }

    [ServerRpc]
    private void HitNearbyTargetsServerRpc()
    {
        HitNearbyTargets();
        BecomeHostileIfTargetWasHit();
    }

    protected override bool IsRightTarget(Collider target)
    {
        return IsNotTheSameObject(target) && IsTargetAlive(target) && IsFromDifferentTeam(target);
    }

    private bool IsNotTheSameObject(Collider target)
    {
        return gameObject != target.gameObject;
    }

    private bool IsTargetAlive(Collider target)
    {
        return !target.GetComponent<HealthController>().IsDead;
    }

    private bool IsFromDifferentTeam(Collider target)
    {
        if (target.gameObject.TryGetComponent<TeamController>(out var targetsTeamController))
            return targetsTeamController.Team != _teamController.Team;
        return true;
    }

    private void BecomeHostileIfTargetWasHit()
    {
        if (_targetHit)
            BecomeHostile();
    }

    private void BecomeHostile()
    {
        HostilePlayerManager.Instance.AddToHostilePlayers(transform);
        _playerHostility.RestartHostileTimer();
    }
}
