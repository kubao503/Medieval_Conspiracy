using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerState : NetworkBehaviour, IDead
{
    public static PlayerState LocalInstance;
    public event EventHandler StateUpdated;

    [SerializeField] private float _hostileDuration;
    private readonly NetworkVariable<State> _playerNetState = new(value: State.INSIDE);
    private PlayerController _playerController;
    private Coroutine _hostileTimerCo;
    private bool _isHostile = false;

    public enum State : byte
    {
        OUTSIDE,
        INSIDE,
        ON_PATH,
        DEAD
    }

    bool IDead.IsDead() => _playerNetState.Value == State.DEAD;


    public bool IsHostile => _isHostile;


    public State CurrentState {
        get => _playerNetState.Value;
        set
        {
            _playerNetState.Value = value;
            SetNewStateServerRpc(value);
        }
    }


    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }


    public override void OnNetworkSpawn()
    {
        // Subscribe to state changes
        _playerNetState.OnValueChanged += StateUpdate;
        StateUpdate(_playerNetState.Value, _playerNetState.Value);
        //_playerController.Disappear();

        if (IsOwner)
            LocalInstance = this;
    }

    private void StateUpdate(State oldState, State newState)
    {
        StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);
        }

        base.OnDestroy();
    }


    public void RestartHostileTimer()
    {
        StopHostileTimer();
        _hostileTimerCo = StartCoroutine(HostileTimerCo());
    }


    // Safe way to stop _hostileTimerCo
    // Checks if its really running
    public void StopHostileTimer()
    {
        if (_isHostile)
        {
            StopCoroutine(_hostileTimerCo);
            _isHostile = false;
        }
    }


    public IEnumerator HostileTimerCo()
    {
        _isHostile = true;
        yield return new WaitForSeconds(_hostileDuration);
        _isHostile = false;

        // Try to remove from hostile players
        if (!GuardManager.Instance.IsRaidActive())
            HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNewStateServerRpc(State newState)
    {
        _playerNetState.Value = newState;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (_playerNetState.Value == State.DEAD)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }
}
