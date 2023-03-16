using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class NPCController : NetworkBehaviour
{
    [SerializeField] private LayerMask _NPCLayer;
    [SerializeField] private LayerMask _deadNPCLayer;
    [SerializeField] private float _respawnTime;
    private readonly Vector3 torque = new(.2f, .1f, .2f);
    private Rigidbody _rigidBody;
    private NpcFollower _follower;
    private NpcHealth _npcHealth;
    private AudioSource _audioSource;

    public void Panic(Vector3 dangerPosition)
    {
        _follower.RunAwayFromDanger(dangerPosition);
        Scream();
    }

    public void SetSpeedToDefault()
    {
        _follower.SetSpeedToDefault();
    }

    public void DistanceSync()
    {
        _follower.DistanceSync();
    }

    private void Awake()
    {
        _follower = GetComponent<NpcFollower>();
        _npcHealth = GetComponent<NpcHealth>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SubscribeToDeadUpdate();
        if (IsServer)
            _follower.SetRandomPositionAlongPath();
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
        _follower.SetRandomPositionAlongPath();
        _follower.DistanceSync();
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

    private void Scream()
    {
        if (IsAudioIdle())
            _audioSource.Play();
    }

    private bool IsAudioIdle()
    {
        return !_audioSource.isPlaying;
    }
}
