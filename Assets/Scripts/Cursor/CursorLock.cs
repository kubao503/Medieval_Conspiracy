using UnityEngine;
using Unity.Netcode;


[RequireComponent(typeof(PlayerState))]
public class CursorLock : NetworkBehaviour
{
    private PlayerState _playerState;
    private IInput _input = InputAdapter.Instance;
    private const KeyCode _cursorReleaseKey = KeyCode.LeftControl;

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            _playerState.StateUpdated += StateUpdated;
        }
        else
            this.enabled = false;

        base.OnNetworkSpawn();
    }

    private void StateUpdated(object sender, StateEventArgs args)
    {
        if (IsPlayerDead())
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_input.GetKeyDown(_cursorReleaseKey))
            Cursor.lockState = CursorLockMode.None;
        else if (_input.GetKeyUp(_cursorReleaseKey))
            Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnApplicationFocus(bool focus)
    {
        SetCursorLockBasedOnState();
    }

    private void SetCursorLockBasedOnState()
    {
        if (IsPlayerDead())
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    private bool IsPlayerDead()
    {
        return _playerState.CurrentState == PlayerState.State.Dead;
    }
}
