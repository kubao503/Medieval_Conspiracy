using Unity.Netcode;
using UnityEngine;

using State = PlayerState.State;


public class PlayerController : NetworkBehaviour
{
    public static GameObject LocalPlayer;
    private readonly NetworkVariable<NetworkTransform> _netTransform = new(writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _vaultLayer;
    private AudioListener _audioListener;
    private PlayerHealth _playerHealth;
    private PlayerState _playerState;
    private PlayerHostility _playerHostility;
    private TeamController _teamController;
    private BaseInteractions _baseInteractions;
    private Rigidbody _rb;
    private Renderer[] _renderers;
    private int _money = 0;
    private IInput _input = InputAdapter.Instance;

    private const RigidbodyConstraints default_constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    private readonly Vector3 _moneyCollectionCenter = new(0f, -.5f, 0f);
    private const float _spawnHeight = 1f;
    private const float _moneyCollectionRadius = .5f;
    private const int _moneyCollection = 1;
    private const int _maxMoney = 5;

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
            case (_, State.Dead):
                Die();
                break;
        }
    }

    private void Die()
    {
        Disappear();

        if (IsServer)
        {
            HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);
            _playerHostility.StopHostileTimer();

            TeamManager.Instance.DeadPlayerUpdate(_teamController.Team, OwnerClientId);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalPlayer = this.gameObject;
        else
        {
            _audioListener.enabled = false;
            _camera.enabled = false;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            // Quit game
            if (_input.GetKeyDown(KeyCode.Escape))
                Quit();

            // Entering entrance
            if (_input.GetKeyDown(KeyCode.E))
                CollectMoney();

            UpdateNetPositionAndRotation();
        }
        else
        {
            // Synchronize position and rotation
            transform.SetPositionAndRotation(_netTransform.Value.Position, _netTransform.Value.Rotation);
        }
    }

    private void UpdateNetPositionAndRotation()
    {
        _netTransform.Value = new NetworkTransform()
        {
            Position = transform.position,
            Rotation = transform.rotation
        };
    }

    private void LeaveMoney(BaseController baseController)
    {
        // Money
        baseController.LeaveMoney(_money);
        _money = 0;
        MainUIController.Instance.UpdateMoneyText(_money);
    }


    private void CollectMoney()
    {
        // Find nearby vaults
        Collider[] vaults = Physics.OverlapSphere(transform.TransformPoint(_moneyCollectionCenter), _moneyCollectionRadius, _vaultLayer);

        if (vaults.Length == 0) return; // No money to collect

        // Add money for player
        _money = Mathf.Clamp(_money + _moneyCollection, min: 0, max: _maxMoney);
        MainUIController.Instance.UpdateMoneyText(_money);
    }


    public void Respawn()
    {
        //_playerState.CurrentState = State.OUTSIDE;

        _playerHealth.RegainHealthServerRpc();
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

    private void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.yellow;

        // Entrance detection sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.TransformPoint(_moneyCollectionCenter), _moneyCollectionRadius);
    }
}
