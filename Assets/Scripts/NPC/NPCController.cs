using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class NPCController : NetworkBehaviour
{
    [SerializeField] private LayerMask _NPCLayer;
    [SerializeField] private LayerMask _deadNPCLayer;
    [SerializeField] private float _respawnTime;
    [SerializeField] private float _spawnRange;
    [SerializeField] private float _minSpeed;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _runSpeed;
    private readonly Vector3 torque = new(.2f, .1f, .2f);
    private Rigidbody _rigidBody;
    private Follower _follower;
    private NpcHealth _npcHealth;
    private AudioSource _audioSource;
    private float _defaultSpeed;

    private void Awake()
    {
        _follower = GetComponent<Follower>();
        _npcHealth = GetComponent<NpcHealth>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SubscribeToDeadUpdate();
        if (IsServer)
            SetRandomSpeedAndPosition();
    }

    private void SetRandomSpeedAndPosition()
    {
        SetRandomSpeed();
        SetRandomPositionAlongPath();
    }

    private void SetRandomSpeed()
    {
        var defaultSpeed = GetRandomSpeed();

        _defaultSpeed = Mathf.Abs(defaultSpeed);
        _follower.Speed = defaultSpeed;
    }

    private float GetRandomSpeed()
    {
        var randomSpeed = Random.Range(_minSpeed, _maxSpeed);
        if (Random.value < .5)
            randomSpeed *= -1; // Direction
        return randomSpeed;
    }

    private void SetRandomPositionAlongPath()
    {
        var randomDistance = Random.Range(0f, _follower.PathLength);
        var randomOffset = Random.Range(-_spawnRange, _spawnRange);
        _follower.StartAtGivenPosition(randomOffset, randomDistance);
    }

    private void SubscribeToDeadUpdate()
    {
        _npcHealth.DeadUpdated += DeadUpdate;
    }

    private void DeadUpdate(object sender, DeadEventArgs args)
    {
        if (args.IsDead)
            Die();
        else
            StandUp();
    }

    // Server-side
    private void Die()
    {
        FallDown();

        if (IsServer)
            StartCoroutine(RespawnCoroutine());
    }

    private void FallDown()
    {
        _follower.enabled = false;
        _rigidBody = gameObject.AddComponent<Rigidbody>();
        _rigidBody.AddTorque(torque, ForceMode.VelocityChange);
        gameObject.layer = GetLayerFromLayerMask(_deadNPCLayer);
    }

    // Server-side
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(_respawnTime);
        Respawn();
    }

    private void Respawn()
    {
        StandUp();
        _npcHealth.RegainHealth();
        SetRandomSpeedAndPosition();
        DistanceSync();
        OffsetSyncClientRpc(_follower.Offset);
    }

    private void StandUp()
    {
        _follower.enabled = true;
        Destroy(_rigidBody);
        gameObject.layer = GetLayerFromLayerMask(_NPCLayer);
    }

    private int GetLayerFromLayerMask(LayerMask layerMask)
    {
        return (int)Mathf.Log(layerMask, 2);
    }

    public void Panic(Vector3 dangerPosition)
    {
        RunAwayFrom(dangerPosition);
        Scream();
    }

    private void RunAwayFrom(Vector3 dangerPosition)
    {
        var directionToNPC = transform.position - dangerPosition;
        var angle = Vector3.Angle(_follower.Direction, directionToNPC);
        _follower.Speed = _runSpeed * (angle < 90f ? 1f : -1f);
    }

    private void Scream()
    {
        if (IsAudioIdle())
            _audioSource.Play();
    }

    private bool IsAudioIdle()
    {
        return !_audioSource.isPlaying;
    }

    public void SetToDefaultSpeed()
    {
        _follower.SetSpeed(_defaultSpeed);
    }

    public void DistanceSync()
    {
        DistanceSyncClientRpc(_follower.Distance);
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void DistanceSyncClientRpc(float distance)
    {
        if (!IsOwner)
            _follower.Distance = distance;
    }


    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void OffsetSyncClientRpc(float offset)
    {
        if (!IsOwner)
            _follower.Offset = offset;
    }
}
