using System.Collections;
using UnityEngine;
using Unity.Netcode;


public class GuardAttacker : NetworkBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private float _attackRange;
    [SerializeField] private LayerMask _playerLayer;
    private Coroutine _attackPlayerCoroutine;
    private NpcHealth _npcHealth;
    private GuardAnimator _guardAnimator;
    private bool _playerHit = false;

    private void Awake()
    {
        _npcHealth = GetComponent<NpcHealth>();
        _guardAnimator = GetComponent<GuardAnimator>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartAttackOnPlayer();
            _npcHealth.DeadUpdated += DeadUpdate;
        }
        base.OnNetworkSpawn();
    }

    private void DeadUpdate(object sender, DeadEventArgs args)
    {
        if (args.IsDead)
            StopCoroutine(_attackPlayerCoroutine);
    }

    private void StartAttackOnPlayer()
    {
        _attackPlayerCoroutine = StartCoroutine(AttackPlayerCoroutine());
    }

    IEnumerator AttackPlayerCoroutine()
    {
        while (true)
        {
            AttackNearbyHostilePlayers();
            PlayAnimationIfPlayerWasHit();
            yield return new WaitForSeconds(1f);
        }
    }

    private void AttackNearbyHostilePlayers()
    {
        Collider[] nearbyPlayers = GetNearbyPlayers();

        _playerHit = false;
        foreach (Collider player in nearbyPlayers)
        {
            if (IsPlayerHostile(player))
                AttackPlayer(player);
        }
    }

    private bool IsPlayerHostile(Collider player)
    {
        return HostilePlayerManager.Instance.IsPlayerHostile(player.transform);
    }

    private void AttackPlayer(Collider player)
    {
        player.GetComponent<PlayerHealth>().TakeDamage(_damage);
        _playerHit = true;
    }

    private Collider[] GetNearbyPlayers()
    {
        return Physics.OverlapSphere(transform.position, _attackRange, _playerLayer);
    }

    private void PlayAnimationIfPlayerWasHit()
    {
        if (_playerHit)
        {
            _guardAnimator.PlayHitAnimation();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
