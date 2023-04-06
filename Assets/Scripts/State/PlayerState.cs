using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;


public interface IPlayerState
{
    public PlayerState.State CurrentState { get; }
}


public class PlayerState : NetworkBehaviour, IPlayerState
{
    public event EventHandler<StateEventArgs> StateUpdated;

    [SerializeField] private float _ragdollDuration = 1f;
    private readonly NetworkVariable<State> _netState = new(_defaultState);
    private const State _defaultState = State.Spawning;

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

    public State CurrentState => _netState.Value;

    [ServerRpc]
    public void RespawnServerRpc()
    {
        Respawn();
    }

    private void Respawn()
    {
        if (_netState.Value == State.Dead)
            _netState.Value = State.Outside;
    }

    public void DeadUpdate(bool dead)
    {
        var isInCorrectState = (CurrentState == State.Outside || CurrentState == State.OnPath);
        if (dead && isInCorrectState)
        {
            _netState.Value = State.Ragdoll;
            StartCoroutine(RagdollCoroutine());
        }
    }

    private IEnumerator RagdollCoroutine()
    {
        yield return new WaitForSeconds(_ragdollDuration);
        _netState.Value = State.Dead;
    }

    [ServerRpc]
    public void ToggleBaseStateServerRpc()
    {
        ToggleBaseState();
    }

    private void ToggleBaseState()
    {
        if (CurrentState == State.Outside || CurrentState == State.TeamSet)
            _netState.Value = State.Inside;
        else if (CurrentState == State.Inside)
            _netState.Value = State.Outside;
    }

    [ServerRpc]
    public void TogglePathFollowingStateServerRpc()
    {
        TogglePathFollowingState();
    }

    private void TogglePathFollowingState()
    {
        if (CurrentState == State.OnPath)
            _netState.Value = State.Outside;
        else if (CurrentState == State.Outside)
            _netState.Value = State.OnPath;
    }

    [ServerRpc]
    public void TeamSetServerRpc()
    {
        TeamSet();
    }

    private void TeamSet()
    {
        if (CurrentState == State.Spawning)
            _netState.Value = State.TeamSet;
    }

    private void Awake()
    {
        _netState.OnValueChanged += StateUpdatedCallback;
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
}


public class StateEventArgs : EventArgs
{
    public PlayerState.State OldState;
    public PlayerState.State NewState;
}



public class FakePlayerState : MonoBehaviour, IPlayerState
{
    public PlayerState.State CurrentState { get; set; }
}
