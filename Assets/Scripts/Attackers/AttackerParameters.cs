using UnityEngine;


[CreateAssetMenu]
public class AttackerParameters : ScriptableObject
{
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private int _damage;
    [SerializeField] private float _attackRange;

    public LayerMask TargetLayer => _targetLayer;
    public int Damage => _damage;
    public float AttackRange => _attackRange;
}
