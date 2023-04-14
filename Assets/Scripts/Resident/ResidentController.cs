using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class ResidentController : NetworkBehaviour
{
    [SerializeField] private float _respawnTime;
    private ResidentFollower _follower;
    private NpcHealth _npcHealth;
    private RagdollController _ragdollController;
    private AudioSource _audioSource;

    public void Panic(Vector3 dangerPosition)
    {
        _follower.RunAwayFromDanger(dangerPosition);
        Scream();
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

    private void Awake()
    {
        _follower = GetComponent<ResidentFollower>();
        _npcHealth = GetComponent<NpcHealth>();
        _ragdollController = GetComponent<RagdollController>();
        _audioSource = GetComponent<AudioSource>();
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

    // Server-side
    private void Die()
    {
        _ragdollController.FallDown();

        _follower.enabled = false;

        if (IsServer)
            StartCoroutine(RespawnCoroutine());
    }

    // Server-side
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(_respawnTime);
        Respawn();
    }

    private void Respawn()
    {
        _ragdollController.StandUp();

        _follower.enabled = true;

        _npcHealth.RegainHealth();
        _follower.SetRandomPositionAlongPath();
        _follower.DistanceSync();
    }
}
