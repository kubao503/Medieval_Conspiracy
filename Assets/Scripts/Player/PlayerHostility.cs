using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerHostility : NetworkBehaviour
{
    [SerializeField] private float _hostileDuration;
    private Coroutine _hostileTimerCo;
    private bool _isHostile = false;

    public bool IsHostile => _isHostile;

    public override void OnDestroy()
    {
        if (IsServer)
            HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);

        base.OnDestroy();
    }

    public void RestartHostileTimer()
    {
        StopHostileTimer();
        StartHostileTimer();
    }

    // Safe way to stop hostileTimer
    public void StopHostileTimer()
    {
        if (_isHostile)
        {
            StopCoroutine(_hostileTimerCo);
            _isHostile = false;
        }
    }

    private void StartHostileTimer()
    {
        _hostileTimerCo = StartCoroutine(HostileTimerCoroutine());
    }

    public IEnumerator HostileTimerCoroutine()
    {
        _isHostile = true;
        yield return new WaitForSeconds(_hostileDuration);
        _isHostile = false;

        TryRemoveFromHostilePlayers();
    }

    private void TryRemoveFromHostilePlayers()
    {
        if (IsRaidOver())
            HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);
    }

    private bool IsRaidOver()
    {
        return !GuardManager.Instance.IsRaidActive();
    }
}
