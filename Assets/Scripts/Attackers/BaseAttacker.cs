using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.Netcode;


public abstract class BaseAttacker : NetworkBehaviour
{
    [SerializeField] private AttackerParameters _parameters;
    protected BaseAnimator _animator;
    protected bool _targetHit = false;

    protected void HitNearbyTargets()
    {
        var nearbyTargets = GetNearbyTargets();

        _targetHit = false;
        foreach (Collider target in nearbyTargets)
        {
            if (IsRightTarget(target))
                HitTarget(target);
        }
    }

    private IEnumerable GetNearbyTargets()
    {
        var boxCenter = GetBoxCenter();
        Collider[] targetsInBox = Physics.OverlapBox(boxCenter, Vector3.one * _parameters.AttackRange, transform.rotation, _parameters.TargetLayer);
        Collider[] targetsInSphere = Physics.OverlapSphere(transform.position, _parameters.AttackRange, _parameters.TargetLayer);

        return Enumerable.Intersect(targetsInSphere, targetsInBox);
    }

    private Vector3 GetBoxCenter()
    {
        return transform.position + transform.forward * _parameters.AttackRange;
    }

    protected abstract bool IsRightTarget(Collider target);

    private void HitTarget(Collider target)
    {
        target.GetComponent<HealthController>().TakeDamage(_parameters.Damage);
        _targetHit = true;
    }

    protected void Awake()
    {
        _animator = GetComponent<BaseAnimator>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(GetBoxCenter(), _parameters.AttackRange * 2 * Vector3.one);
        Gizmos.DrawWireSphere(transform.position, _parameters.AttackRange);
    }
}
