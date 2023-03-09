using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerState : NetworkBehaviour, IDead
{
    private readonly NetworkVariable<State> _playerNetState = new(value: State.INSIDE);

    [SerializeField] private float _hostileDuration;

    private PlayerController _playerController;
    private State _currentState = State.INSIDE;
    private Coroutine _hostileTimerCo;
    private bool _isHostile = false;


    public enum State : byte
    {
        OUTSIDE,
        INSIDE,
        ON_PATH,
        DEAD
    }


    bool IDead.IsDead() => _currentState == State.DEAD;


    public bool IsHostile => _isHostile;


    public State CurrentState {
        get => _currentState;
        set
        {
            _currentState = value;
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
        StateUpdate(_currentState, _currentState);
        //_playerController.Disappear();

        if (IsOwner && _playerController.FindBaseEntrance(out var baseController))
            _playerController.EnterBase(baseController);
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


    private void StateUpdate(State oldState, State newState)
    {
        //Debug.Log("State update " + oldState + " " + newState);

        switch (oldState, newState)
        {
            case (State.DEAD, State.OUTSIDE):
                _playerController.Appear();
                if (IsOwner)
                {
                    _currentState = newState;
                    MainUIController.Instance.ShowDeathInfo(false);
                }
                break;
            case (_, State.INSIDE):
                _playerController.Disappear();
                break;
            case (_, State.OUTSIDE):
                _playerController.Appear();
                break;
            case (_, State.DEAD):
                _playerController.Disappear();
                if (IsOwner)
                {
                    _currentState = newState;
                    MainUIController.Instance.ShowDeathInfo(true);
                }
                break;
        }
    }


    private void OnApplicationFocus(bool focus) => Cursor.lockState = _currentState == State.DEAD ? CursorLockMode.None : CursorLockMode.Locked;
}
