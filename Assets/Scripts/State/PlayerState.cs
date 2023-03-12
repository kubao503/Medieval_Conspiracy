using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerState : NetworkBehaviour
{
    public static PlayerState LocalInstance;
    public event EventHandler StateUpdated;

    private readonly NetworkVariable<State> _playerNetState = new(State.OUTSIDE);

    public enum State : byte
    {
        OUTSIDE,
        INSIDE,
        ON_PATH,
        DEAD
    }

    public State CurrentState
    {
        get => _playerNetState.Value;
        set => SetNewStateServerRpc(value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetNewStateServerRpc(State newState)
    {
        _playerNetState.Value = newState;
    }

    private void Awake()
    {
        _playerNetState.OnValueChanged += StateUpdatedCallback;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalInstance = this;
    }

    private void StateUpdatedCallback(State oldState, State newState)
    {
        StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void OnApplicationFocus(bool focus)
    {
        SetCursorLockBasedOnState();
    }

    private void SetCursorLockBasedOnState()
    {
        if (_playerNetState.Value == State.DEAD)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }
}
