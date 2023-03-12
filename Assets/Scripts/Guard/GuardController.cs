using System.Collections;
using Unity.Netcode;
using UnityEngine;

// Server-side
public class GuardController : NetworkBehaviour
{
    public Transform Target;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _deadGuardLayer;
    [SerializeField] private float _speed;
    [SerializeField] private float _attackRange;
    [SerializeField] private int _damage;
    [SerializeField] private float _respawnTime;
    [SerializeField] private int _animationCount;
    private readonly NetworkVariable<NetworkTransform> _netTransform = new();
    private Rigidbody _rigidBody;
    private Animator _animator;
    private NpcHealth _npcHealth;
    private Coroutine _attackPlayerCo;
    private Vector3 _playerDirection;
    private bool _playerHit = false;
    private readonly Vector3 _torque = new(.2f, .1f, .2f);

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _npcHealth = GetComponent<NpcHealth>();

        SubscribeToDeadUpdate();
        if (IsServer)
            StartAttackOnPlayer();
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

    private void StartAttackOnPlayer()
    {
        _attackPlayerCo = StartCoroutine(AttackPlayerCoroutine());
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

    private Collider[] GetNearbyPlayers()
    {
        return Physics.OverlapSphere(transform.position, _attackRange, _playerLayer);
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

    private void PlayAnimationIfPlayerWasHit()
    {
        if (_playerHit)
        {
            var animationIndex = GetRandomHitAnimationIndex();
            PlayHitAnimationClientRpc(animationIndex);
        }
    }

    private byte GetRandomHitAnimationIndex()
    {
        return (byte)Random.Range(0, _animationCount);
    }

    [ClientRpc]
    private void PlayHitAnimationClientRpc(byte index)
    {
        _animator.SetInteger("Index", index);
        _animator.SetTrigger("Play");
    }


    private void Die()
    {
        FallDown();

        if (IsServer)
        {
            GuardManager.Instance.RemoveFromActiveGuards(gameObject);
            StopCoroutine(_attackPlayerCo);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
