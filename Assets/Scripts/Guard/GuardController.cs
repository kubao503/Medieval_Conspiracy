using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class GuardController : NetworkBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _respawnTime;
    private Rigidbody _rigidBody;
    private NpcHealth _npcHealth;
    private RagdollController _ragdollController;
    private Transform _target;
    private Vector3 _targetDirection;

    public void UpdateTarget()
    {
        _target = HostilePlayerManager.Instance.ClosestTarget(transform.position);
    }

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _npcHealth = GetComponent<NpcHealth>();
        _ragdollController = GetComponent<RagdollController>();
    }

    private void Start()
    {
        SubscribeToDeadUpdate();
    }

    private void SubscribeToDeadUpdate()
    {
        _npcHealth.DeadUpdated += DeadUpdate;
    }

    private void DeadUpdate(object sender, DeadEventArgs args)
    {
        if (args.IsDead)
            Die();
    }

    private void Die()
    {
        _ragdollController.FallDown();

        if (IsServer)
        {
            GuardManager.Instance.RemoveFromActiveGuards(gameObject);
            StartCoroutine(DyingCoroutine());
        }
    }

    private IEnumerator DyingCoroutine()
    {
        yield return new WaitForSeconds(_respawnTime);
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (IsServer && IsAlive())
        {
            TrySetDirectionToTarget();
            MoveInGivenDirection();
            LookAtGivenDirection();
        }
    }

    private bool IsAlive()
    {
        return !_npcHealth.IsDead;
    }

    private void TrySetDirectionToTarget()
    {
        try
        {
            SetDirectionToTarget();
        }
        catch (MissingReferenceException)
        {
            UpdateTarget();
            SetDirectionToTarget();
        }
    }

    private void SetDirectionToTarget()
    {
        _targetDirection = (_target.position - transform.position).normalized;
    }

    private void MoveInGivenDirection()
    {
        _rigidBody.velocity = _targetDirection * _speed;
    }

    private void LookAtGivenDirection()
    {
        transform.LookAt(_target);
    }
}
