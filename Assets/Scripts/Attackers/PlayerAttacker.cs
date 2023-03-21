using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.Netcode;


public class PlayerAttacker : NetworkBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private float _attackRange;
    [SerializeField] private LayerMask _enemyLayer;
    private IInput _input = InputAdapter.Instance;
    private PlayerState _playerState;
    private PlayerHostility _playerHostility;
    private TeamController _teamController;
    private PlayerAnimator _playerAnimator;

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _playerHostility = GetComponent<PlayerHostility>();
        _teamController = GetComponent<TeamController>();
        _playerAnimator = GetComponent<PlayerAnimator>();
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
            AttackServerRpc();
            _playerAnimator.PlayHitAnimation();
        }
    }

    [ServerRpc]
    private void AttackServerRpc()
    {
        var nearbyTargets = GetNearbyTargets();

        foreach (Collider target in nearbyTargets)
        {
            if (IsTheSameObject(target) || IsTargetDead(target) || IsFromTheSameTeam(target))
                continue;

            HitTarget(target);
            BecomeHostile();
        }
    }

    private bool IsTheSameObject(Collider target)
    {
        return gameObject == target.gameObject;
    }

    private bool IsTargetDead(Collider target)
    {
        return target.GetComponent<HealthController>().IsDead;
    }

    private bool IsFromTheSameTeam(Collider target)
    {
        if (target.gameObject.TryGetComponent<TeamController>(out var otherTeamController))
            return otherTeamController.Team == _teamController.Team;
        return false;
    }

    private void HitTarget(Collider target)
    {
        target.GetComponent<HealthController>().TakeDamage(_damage);
    }

    private void BecomeHostile()
    {
        HostilePlayerManager.Instance.AddToHostilePlayers(transform);
        _playerHostility.RestartHostileTimer();
    }

    private IEnumerable GetNearbyTargets()
    {
        var boxCenter = GetBoxCenter();
        Collider[] targetsInBox = Physics.OverlapBox(boxCenter, Vector3.one * _attackRange, transform.rotation, _enemyLayer);
        Collider[] targetsInSphere = Physics.OverlapSphere(transform.position, _attackRange, _enemyLayer);

        return Enumerable.Intersect(targetsInSphere, targetsInBox);
    }

    private Vector3 GetBoxCenter()
    {
        return transform.position + transform.forward * _attackRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(GetBoxCenter(), _attackRange * 2 * Vector3.one);
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
