using System.Collections;
using Unity.Netcode;
using UnityEngine;

// Server-side
public class GuardController : Mortal, IDead
{
    private readonly NetworkVariable<NetworkTransform> _netTransform = new();
    private readonly NetworkVariable<bool> _netDead = new(false);


    public Transform Target;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _deadGuardLayer;
    [SerializeField] private float _speed;
    [SerializeField] private float _attackRange;
    [SerializeField] private int _damage;
    [SerializeField] private float _respawnTime;
    [SerializeField] private int _animationCount;
    private Rigidbody _rigidBody;
    private Animator _animator;
    private Coroutine _attackPlayerCo;
    private Vector3 _playerDirection;
    private bool _playerHit = false;
    private readonly Vector3 _torque = new(.2f, .1f, .2f);

    private new void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();

        if (IsServer)
            StartAttackOnPlayer();
        else
            SubscribeToDeadStatus();

        base.Start();
    }

    private void StartAttackOnPlayer()
    {
        _attackPlayerCo = StartCoroutine(AttackPlayerCoroutine());
    }

    private void SubscribeToDeadStatus()
    {
        _netDead.OnValueChanged += DeadUpdate;
        DeadUpdate(_netDead.Value, _netDead.Value);
    }

    private void DeadUpdate(bool _, bool isDead)
    {
        if (isDead)
            FallDown();
    }

    void Update()
    {
        if (IsServer)
            UpdateNetPositionAndRotation();
        else if (IsAlive())
            transform.SetPositionAndRotation(_netTransform.Value.Position, _netTransform.Value.Rotation);
    }

    private void UpdateNetPositionAndRotation()
    {
        _netTransform.Value = new NetworkTransform()
        {
            Position = transform.position,
            Rotation = transform.rotation
        };
    }

    private bool IsAlive()
    {
        return !_netDead.Value;
    }

    bool IDead.IsDead() => IsAlive();

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
        player.GetComponent<Mortal>().TakeDamage(_damage);
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


    protected override void Die()
    {
        GuardManager.Instance.RemoveFromActiveGuards(gameObject);

        _netDead.Value = true;
        FallDown();

        StopCoroutine(_attackPlayerCo);
        StartCoroutine(DyingCoroutine());
    }


    private IEnumerator DyingCoroutine()
    {
        yield return new WaitForSeconds(_respawnTime);
        Destroy(gameObject);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
