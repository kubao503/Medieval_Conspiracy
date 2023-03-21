using System.Collections;
using Unity.Netcode;
using UnityEngine;

// Server-side
public class GuardController : NetworkBehaviour
{
    public Transform Target;
    [SerializeField] private LayerMask _deadGuardLayer;
    [SerializeField] private float _speed;
    [SerializeField] private float _respawnTime;
    private readonly NetworkVariable<NetworkTransform> _netTransform = new();
    private Rigidbody _rigidBody;
    private NpcHealth _npcHealth;
    private Vector3 _playerDirection;
    private readonly Vector3 _torque = new(.2f, .1f, .2f);

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _npcHealth = GetComponent<NpcHealth>();

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
                SetDirectionToPlayer();
            }
            catch (MissingReferenceException)
            {
                SetDirectionForward();
            }
            MoveInGivenDirection();
            LookAtPlayer();
        }
    }

    private void SetDirectionToPlayer()
    {
        _playerDirection = (Target.position - transform.position).normalized;
    }

    private void SetDirectionForward()
    {
        _playerDirection = transform.forward;
    }

    private void MoveInGivenDirection()
    {
        _rigidBody.velocity = _playerDirection * _speed;
    }

    private void LookAtPlayer()
    {
        transform.LookAt(Target);
    }

    private void Die()
    {
        FallDown();

        if (IsServer)
        {
            GuardManager.Instance.RemoveFromActiveGuards(gameObject);
            StartCoroutine(DyingCoroutine());
        }
    }

    private void FallDown()
    {
        _rigidBody.constraints = RigidbodyConstraints.None;
        _rigidBody.AddTorque(_torque, ForceMode.VelocityChange);
        gameObject.layer = GetLayerFromLayerMask(_deadGuardLayer);
    }

    private int GetLayerFromLayerMask(LayerMask layerMask)
    {
        return (int)Mathf.Log(layerMask, 2);
    }

    private IEnumerator DyingCoroutine()
    {
        yield return new WaitForSeconds(_respawnTime);
        Destroy(gameObject);
    }
}
