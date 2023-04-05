using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class GuardManager : NetworkBehaviour
{
    public static GuardManager Instance;

    // All alive guards taking part in raid
    private readonly HashSet<GameObject> _activeGuards = new();
    [SerializeField] private float _cooldownDuration;
    [SerializeField] private int _guardCount;
    [SerializeField] private float _updateTargetsInterval = 1f;
    private GuardSpawner _guardSpawner;
    private Coroutine _updateTargetsCoroutine;
    private WaitForSeconds _updateTargetsWait;
    private bool _raidActive = false;
    private bool _raidCooldown = false;

    public bool IsRaidActive() => _raidActive;

    public void TryStartRaid(Vector3 playerPosition)
    {
        if (!IsRaidOrCooldownActive())
            StartGuardRaid(playerPosition);
    }

    private bool IsRaidOrCooldownActive()
    {
        return _raidActive || _raidCooldown;
    }

    private void StartGuardRaid(Vector3 playerPosition)
    {
        _raidActive = true;

        var playerDistanceOnPath = MainPath.Path.GetClosestDistanceAlongPath(playerPosition);
        SpawnAllGuards(playerDistanceOnPath);
        StartUpdatingTargets();
    }

    private void SpawnAllGuards(float playerDistanceOnPath)
    {
        for (int i = 0; i < _guardCount; ++i)
        {
            var guard = _guardSpawner.Spawn(playerDistanceOnPath);
            _activeGuards.Add(guard);
        }
    }

    private void StartUpdatingTargets()
    {
        _updateTargetsCoroutine = StartCoroutine(UpdateTargetsCoroutine());
    }

    IEnumerator UpdateTargetsCoroutine()
    {
        while (_raidActive)
        {
            UpdateTargetsForAllGuards();
            yield return _updateTargetsWait;
        }
    }

    private void UpdateTargetsForAllGuards()
    {
        foreach (var guard in _activeGuards)
            UpdateTarget(guard);
    }

    private void UpdateTarget(GameObject guard)
    {
        var guardController = guard.GetComponent<GuardController>();
        guardController.UpdateTarget();
    }

    public void TryEndRaid()
    {
        if (_raidActive)
            EndGuardRaid();
    }

    private void EndGuardRaid()
    {
        _raidActive = false;
        DestroyAllGuards();
        StopCoroutine(_updateTargetsCoroutine);
    }

    private void DestroyAllGuards()
    {
        foreach (var guard in _activeGuards)
            Destroy(guard);
        _activeGuards.Clear();
    }

    public void RemoveFromActiveGuards(GameObject guard)
    {
        _activeGuards.Remove(guard);
        CheckForAllGuardsDead();
    }

    private void CheckForAllGuardsDead()
    {
        if (AreAllGuardsDead() && _raidActive)
        {
            EndGuardRaid();
            StartCoroutine(RaidCooldownTimerCoroutine());

            HostilePlayerManager.Instance.RemoveNonHostilePlayers();
        }
    }

    private bool AreAllGuardsDead()
    {
        return _activeGuards.Count == 0;
    }

    private IEnumerator RaidCooldownTimerCoroutine()
    {
        _raidCooldown = true;
        yield return new WaitForSeconds(_cooldownDuration);

        _raidCooldown = false;
        StartGuardRaidIfThereAreHostilePlayers();
    }

    private void StartGuardRaidIfThereAreHostilePlayers()
    {
        if (HostilePlayerManager.Instance.GetHostilePlayerPositionIfThereAreAny(out var playerPosition))
            StartGuardRaid(playerPosition);
    }

    private void Awake()
    {
        Instance = this;
        _guardSpawner = GetComponent<GuardSpawner>();
        _updateTargetsWait = new WaitForSeconds(_updateTargetsInterval);
    }
}
