using System;
using Unity.Netcode;
using UnityEngine;

using State = PlayerState.State;


public class PlayerController : NetworkBehaviour
{
    public static GameObject LocalPlayer;
    [SerializeField] private Camera _camera;
    private AudioListener _audioListener;
    private PlayerHealth _playerHealth;
    private PlayerState _playerState;
    private PlayerHostility _playerHostility;
    private TeamController _teamController;
    private BaseInteractions _baseInteractions;
    private Rigidbody _rb;
    private Renderer[] _renderers;
    private RagdollController _ragdollController;
    private IInput _input = InputAdapter.Instance;

    private const RigidbodyConstraints default_constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    private const float _spawnHeight = 1f;

    private void Awake()
    {
        GetComponents();
        SubscribeToEvents();
    }

    private void GetComponents()
    {
        _rb = GetComponent<Rigidbody>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        _audioListener = GetComponent<AudioListener>();
        _playerHealth = GetComponent<PlayerHealth>();
        _playerState = GetComponent<PlayerState>();
        _playerHostility = GetComponent<PlayerHostility>();
        _teamController = GetComponent<TeamController>();
        _baseInteractions = GetComponent<BaseInteractions>();
        _ragdollController = GetComponent<RagdollController>();
    }

    private void SubscribeToEvents()
    {
        _playerState.StateUpdated += StateUpdated;
    }

    private void StateUpdated(object sender, StateEventArgs args)
    {
        switch (args.OldState, args.NewState)
        {
            case (_, State.Inside):
                EnterBase();
                break;
            case (State.Inside, State.Outside):
                LeaveBase();
                break;
            case (_, State.Ragdoll):
                BecomeRagdoll();
                break;
            case (_, State.Dead):
                Die();
                break;
            case (State.Dead, State.Outside):
                Appear();
                break;
        }
    }

    private void BecomeRagdoll()
    {
        _ragdollController.FallDown();
        if (IsServer)
        {
            HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);
            _playerHostility.StopHostileTimer();
        }
    }

    private void Die()
    {
        Disappear();
        if (IsServer)
            TeamManager.Instance.DeadPlayerUpdate(_teamController.Team, OwnerClientId);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this.gameObject;
            MainUIController.Instance.RespawnClicked += RespawnCallback;
        }
        else
        {
            _audioListener.enabled = false;
            _camera.enabled = false;
        }
    }

    private void RespawnCallback(object sender, EventArgs args)
    {
        _ragdollController.StandUp();
        _playerState.RespawnServerRpc();
        _playerHealth.RegainHealthServerRpc();
    }

    private void Update()
    {
        if (IsOwner && _input.GetKeyDown(KeyCode.Escape))
            Quit();
    }

    public void Quit()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void Appear()
    {
        foreach (var renderer in _renderers) renderer.enabled = true;
        _rb.detectCollisions = true;
        _rb.constraints = default_constraints;
    }

    private void LeaveBase()
    {
        Appear();

        if (IsOwner)
        {
            SwitchCameras();
            SetLeavingPosition();
            SetLeavingRotation();
        }
    }

    private void SwitchCameras()
    {
        var insideBase = _playerState.CurrentState == State.Inside;
        _baseInteractions.BaseController.EnableEntranceCamera(insideBase);
        _camera.enabled = !insideBase;
    }

    private void SetLeavingPosition()
    {
        var entranceTransform = _baseInteractions.BaseController.EntranceTransform;
        var leaving_position = entranceTransform.position;
        leaving_position.y = _spawnHeight;
        transform.position = leaving_position;
    }

    private void SetLeavingRotation()
    {
        var entranceTransform = _baseInteractions.BaseController.EntranceTransform;
        transform.rotation = entranceTransform.rotation;
    }

    public void Disappear()
    {
        foreach (var renderer in _renderers) renderer.enabled = false;
        _rb.detectCollisions = false;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void EnterBase()
    {
        Disappear();

        if (IsOwner)
            SwitchCameras();

        if (IsServer)
            RemoveFromHostilePlayers();
    }

    private void RemoveFromHostilePlayers()
    {
        HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);
    }
}
