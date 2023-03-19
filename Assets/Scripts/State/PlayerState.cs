using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;

public class PlayerState : NetworkBehaviour
{
    public event EventHandler<StateEventArgs> StateUpdated;

    private readonly NetworkVariable<State> _playerNetState = new(_defaultState);
    private const State _defaultState = State.Spawning;
    private const float _ragdollDuration = 3f;

    public enum State : byte
    {
        Spawning,
        TeamSet,
        Inside,
        Outside,
        OnPath,
        Ragdoll,
        Dead
    }

    public State CurrentState => _playerNetState.Value;

    public void DeadUpdate(bool dead)
    {
        var isInCorrectState = CurrentState == State.Outside || CurrentState == State.OnPath;
        if (dead && isInCorrectState)
        {
            _playerNetState.Value = State.Ragdoll;
            StartCoroutine(RagdollCoroutine());
        }
    }

    private IEnumerator RagdollCoroutine()
    {
        yield return new WaitForSeconds(_ragdollDuration);
        _playerNetState.Value = State.Dead;
    }

    [ServerRpc]
    public void ToggleBaseStateServerRpc()
    {
        ToggleBaseState();
    }

    private void ToggleBaseState()
    {
        if (CurrentState == State.Outside || CurrentState == State.TeamSet)
            _playerNetState.Value = State.Inside;
        else if (CurrentState == State.Inside)
            _playerNetState.Value = State.Outside;
    }

    [ServerRpc]
    public void TogglePathFollowingStateServerRpc()
    {
        TogglePathFollowingState();
    }

    public void TogglePathFollowingState()
    {
        if (CurrentState == State.OnPath)
            _playerNetState.Value = State.Outside;
        else if (CurrentState == State.Outside)
            _playerNetState.Value = State.OnPath;
    }

    [ServerRpc]
    public void TeamSetServerRpc()
    {
        TeamSet();
    }

    private void TeamSet()
    {
        if (CurrentState == State.Spawning)
            _playerNetState.Value = State.TeamSet;
    }

    private void Awake()
    {
        _playerNetState.OnValueChanged += StateUpdatedCallback;
    }

    private void StateUpdatedCallback(State oldState, State newState)
    {
        var args = new StateEventArgs()
        {
            OldState = oldState,
            NewState = newState
        };
        StateUpdated?.Invoke(this, args);
    }

    // TODO: Remove following from here
    private void OnApplicationFocus(bool focus)
    {
        SetCursorLockBasedOnState();
    }

    private void SetCursorLockBasedOnState()
    {
        if (_playerNetState.Value == State.Dead)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }
}


public class StateEventArgs : EventArgs
{
    public PlayerState.State OldState;
    public PlayerState.State NewState;
}
