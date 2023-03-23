using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class GuardController : NetworkBehaviour
{
    public Transform Target;

    [SerializeField] private float _speed;
    [SerializeField] private float _respawnTime;
    private readonly NetworkVariable<NetworkTransform> _netTransform = new();
    private Rigidbody _rigidBody;
    private NpcHealth _npcHealth;
    private RagdollController _ragdollController;
    private Vector3 _targetDirection;

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

    void Update()
    {
        if (IsServer)
            SetNetTransform();
        else if (IsAlive())
            SetTransformBasedOnNetTransform();
    }

    private void SetNetTransform()
    {
        _netTransform.Value = new NetworkTransform()
        {
            Position = transform.position,
            Rotation = transform.rotation
        };
    }

    private bool IsAlive()
    {
        return !_npcHealth.IsDead;
    }

    private void SetTransformBasedOnNetTransform()
    {
        transform.SetPositionAndRotation(
            _netTransform.Value.Position,
            _netTransform.Value.Rotation);
    }

    private void FixedUpdate()
    {
        if (IsServer && IsAlive())
        {
            try
            {
                SetDirectionToTarget();
            }
            catch (MissingReferenceException)
            {
                SetDirectionForward();
            }
            MoveInGivenDirection();
            LookAtTarget();
        }
    }

    private void SetDirectionToTarget()
    {
        _targetDirection = (Target.position - transform.position).normalized;
    }

    private void SetDirectionForward()
    {
        _targetDirection = transform.forward;
    }

    private void MoveInGivenDirection()
    {
        _rigidBody.velocity = _targetDirection * _speed;
    }

    private void LookAtTarget()
    {
        transform.LookAt(Target);
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
}
