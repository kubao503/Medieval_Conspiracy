using System.Collections;
using UnityEngine;


// Here 'Target' means 'Player'
public class GuardAttacker : BaseAttacker
{
    private Coroutine _attackTargetCoroutine;
    private NpcHealth _npcHealth;

    private new void Awake()
    {
        _npcHealth = GetComponent<NpcHealth>();
        base.Awake();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _npcHealth.DeadUpdated += DeadUpdate;
            StartAttackOnTarget();
        }
        base.OnNetworkSpawn();
    }

    private void DeadUpdate(object sender, DeadEventArgs args)
    {
        if (args.IsDead)
            StopCoroutine(_attackTargetCoroutine);
    }

    private void StartAttackOnTarget()
    {
        _attackTargetCoroutine = StartCoroutine(AttackTargetCoroutine());
    }

    IEnumerator AttackTargetCoroutine()
    {
        while (true)
        {
            HitNearbyTargets();
            PlayAnimationIfTargetWasHit();
            yield return new WaitForSeconds(1f);
        }
    }

    protected override bool IsRightTarget(Collider target)
    {
        return IsTargetHostile(target);
    }

    private bool IsTargetHostile(Collider target)
    {
        return HostilePlayerManager.Instance.IsPlayerHostile(target.transform);
    }

    private void PlayAnimationIfTargetWasHit()
    {
        if (_targetHit)
            _animator.PlayHitAnimation();
    }
}
