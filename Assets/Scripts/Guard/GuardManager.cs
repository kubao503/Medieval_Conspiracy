using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Server-side
public class GuardManager : NetworkBehaviour
{
    public static GuardManager Instance;

    // All alive guards taking part in raid
    private readonly HashSet<GameObject> _activeGuards = new();

    [SerializeField] private GameObject _guardPrefab;
    [SerializeField] private float _cooldownDuration;
    [SerializeField] private float _spawnRadius;
    [SerializeField] private int _guardCount;
    private Coroutine _updateTargetsCo;
    private bool _raidActive = false;
    private bool _raidCooldown = false;


    private void Awake()
    {
        Instance = this;
    }


    public bool IsRaidActive() => _raidActive;


    public void RemoveFromActiveGuards(GameObject guard)
    {
        _activeGuards.Remove(guard);
        CheckForAllGuardsDead();
    }


    private void CheckForAllGuardsDead()
    {
        if (_activeGuards.Count == 0 && _raidActive)
        {
            EndGuardRaid();
            StartCoroutine(RaidCooldownTimerCo());

            // Remove non-hostile players
            HostilePlayerManager.Instance.RemoveNonHostilePlayers();
        }
    }


    public void TryStartRaid(Vector3 playerPosition)
    {
        if (!_raidActive && !_raidCooldown)
        {
            StartGuardRaid(playerPosition);
        }
    }


    private void StartGuardRaid(Vector3 playerPosition)
    {
        _raidActive = true;

        var playerDistanceOnPath = Follower.NpcPath.path.GetClosestDistanceAlongPath(playerPosition);
        for (int i = 0; i < _guardCount; ++i)
            SpawnGuard(playerDistanceOnPath);
        _updateTargetsCo = StartCoroutine(UpdateTargetsCo());
    }


    public void TryEndRaid()
    {
        if (_raidActive) EndGuardRaid();
    }


    private void EndGuardRaid()
    {
        _raidActive = false;
        foreach (var guard in _activeGuards) Destroy(guard);
        _activeGuards.Clear();
        StopCoroutine(_updateTargetsCo);
    }


    // Updates targets for all active guards
    IEnumerator UpdateTargetsCo()
    {
        while (_raidActive)
        {
            UpdateTargets();
            yield return new WaitForSeconds(0.1f);
        }
    }


    private void UpdateTargets()
    {
        foreach (var guard in _activeGuards) UpdateTarget(guard);
    }


    private void UpdateTarget(GameObject guard)
    {
        guard.GetComponent<GuardController>().Target = HostilePlayerManager.Instance.ClosestTarget(guard.transform.position).transform;
    }


    private IEnumerator RaidCooldownTimerCo()
    {
        _raidCooldown = true;
        yield return new WaitForSeconds(_cooldownDuration);
        _raidCooldown = false;

        // Check for hostile players
        if (HostilePlayerManager.Instance.CheckForHostilePlayers(out var playerPosition))
        {
            // Span guards near random hostile player
            StartGuardRaid(playerPosition);
        }
    }


    private void SpawnGuard(float playerDistanceOnPath)
    {
        //var circle2D = Random.insideUnitCircle.normalized;
        //var circle3D = new Vector3(circle2D.x, 0f, circle2D.y);

        // Set position
        // Distance on path relative to player
        var guardRelativeDistance = (Random.value < .5) ? _spawnRadius : -_spawnRadius;
        
        // Distance on path
        var distance = playerDistanceOnPath + guardRelativeDistance;

        // Distance on path in world space
        var position = Follower.NpcPath.path.GetPointAtDistance(distance);
        position.y = _guardPrefab.transform.position.y;

        //newGuard.transform.position = circle3D * _spawnRadius + playerPosition;
        var newGuard = Instantiate(_guardPrefab, position, Quaternion.identity);

        // Set target
        UpdateTarget(newGuard);

        newGuard.GetComponent<NetworkObject>().Spawn();

        _activeGuards.Add(newGuard);
    }
}
