using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.Netcode;


public abstract class BaseAttacker : NetworkBehaviour
{
    [SerializeField] protected LayerMask _targetLayer;
    [SerializeField] private int _damage;
    [SerializeField] private float _attackRange;
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
        Collider[] targetsInBox = Physics.OverlapBox(boxCenter, Vector3.one * _attackRange, transform.rotation, _targetLayer);
        Collider[] targetsInSphere = Physics.OverlapSphere(transform.position, _attackRange, _targetLayer);

        return Enumerable.Intersect(targetsInSphere, targetsInBox);
    }

    private Vector3 GetBoxCenter()
    {
        return transform.position + transform.forward * _attackRange;
    }

    protected abstract bool IsRightTarget(Collider target);

    private void HitTarget(Collider target)
    {
        target.GetComponent<HealthController>().TakeDamage(_damage);
        _targetHit = true;
    }

    protected void Awake()
    {
        _animator = GetComponent<BaseAnimator>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(GetBoxCenter(), _attackRange * 2 * Vector3.one);
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
