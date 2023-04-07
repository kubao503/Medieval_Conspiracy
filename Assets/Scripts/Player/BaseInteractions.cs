using UnityEngine;
using Unity.Netcode;


public class BaseInteractions : NetworkBehaviour
{
    [SerializeField] private LayerMask _doorLayer;
    private IInput _input = InputAdapter.Instance;
    private BaseController _baseController;
    private TeamController _teamController;
    private PlayerState _playerState;
    private readonly Vector3 _doorDetectionCenter = new(0f, -.5f, 0f);
    private const float _doorDetectionRadius = .5f;
    private const KeyCode _baseInteractionKey = KeyCode.E;

    public BaseController BaseController => _baseController;

    private void Awake()
    {
        _teamController = GetComponent<TeamController>();
        _playerState = GetComponent<PlayerState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            _playerState.StateUpdated += StateUpdated;
        else
            this.enabled = false;
    }

    private void StateUpdated(object sender, StateEventArgs args)
    {
        if (args.NewState == PlayerState.State.TeamSet)
            DoBaseInteractionAfterTeamSet();
    }

    private void DoBaseInteractionAfterTeamSet()
    {
        BaseInteraction();
        _playerState.StateUpdated -= StateUpdated;
    }

    private void Update()
    {
        if (_input.GetKeyDown(_baseInteractionKey))
            BaseInteraction();
    }

    private void BaseInteraction()
    {
        var success = TryFindBaseDoor();
        if (success)
            ChangePlayerState();
    }

    private bool TryFindBaseDoor()
    {
        Collider[] nearbyDoors = FindNearbyDoors();

        foreach (var door in nearbyDoors)
        {
            if (IsBaseDoorOfMatchingTeam(door.transform))
                return true;
        }
        return false;
    }

    private Collider[] FindNearbyDoors()
    {
        return Physics.OverlapSphere(transform.TransformPoint(_doorDetectionCenter), _doorDetectionRadius, _doorLayer);
    }

    private bool IsBaseDoorOfMatchingTeam(Transform door)
    {
        if (IsBaseDoor(door, out var baseController))
        {
            if (IsTeamMatching(baseController))
            {
                _baseController = baseController;
                return true;
            }
        }
        return false;
    }

    private bool IsBaseDoor(Transform door, out BaseController baseController)
    {
        return door.TryGetComponent<BaseController>(out baseController);
    }

    private bool IsTeamMatching(BaseController baseController)
    {
        return _teamController.IsTeamMatching(baseController.Team);
    }

    private void ChangePlayerState()
    {
        _playerState.ToggleBaseStateServerRpc();
    }
}
