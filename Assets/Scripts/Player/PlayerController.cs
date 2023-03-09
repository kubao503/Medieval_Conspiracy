using System.Linq;
using Unity.Netcode;
using UnityEngine;

using State = PlayerState.State;

public class PlayerController : NetworkBehaviour
{
    private readonly NetworkVariable<NetworkTransform> _netTransform = new(writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField] private GameObject _cam;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private LayerMask _entranceLayer;
    [SerializeField] private LayerMask _vaultLayer;
    [SerializeField] private float _walkingSpeed;
    [SerializeField] private float _pathFollowingSpeed;
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _attackRange;
    [SerializeField] private Transform _attackCenter;
    [SerializeField] private int _damage;
    [SerializeField] private float _cameraMaxAngle;
    [SerializeField] private float _cameraMinAngle;
    [SerializeField] private int _animationCount;
    private PlayerHealth _playerHealth;
    private PlayerState _playerState;
    private TeamController _teamController;
    private CameraMover _cameraController;
    private Rigidbody _rb;
    private Renderer[] _renderers;
    private Animator _animator;
    private Follower _follower;
    private float _cameraVerticalAngle = 0f;
    private int _money = 0;
    private IInput _input = new InputAdapter();

    private const RigidbodyConstraints default_constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    private readonly Vector3 _entranceDetectionCenter = new(0f, -.5f, 0f);
    private const float _spawnHeight = 1f;
    private const float _entranceDetectionRadius = .5f;
    private const int _moneyCollection = 1;
    private const int _maxMoney = 5;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderers = GetComponentsInChildren<Renderer>();
        _animator = GetComponent<Animator>();
        _playerHealth = GetComponent<PlayerHealth>();
        _playerState = GetComponent<PlayerState>();
        _teamController = GetComponent<TeamController>();
        _cameraController = GetComponent<CameraMover>();
        _follower = GetComponent<Follower>();
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            //MainUIController.Instance.SubscribeToRespawnClick(Respawn);
        }
        else
            Destroy(_cam); // Remove camera
    }


    private void Update()
    {
        if (IsOwner)
        {
            // Quit game
            if (_input.GetKeyDown(KeyCode.Escape)) Quit();

            // Entering entrance
            if (_input.GetKeyDown(KeyCode.E)) if (!BaseInteraction()) CollectMoney();

            // Fight
            if (_input.GetLeftMouseButtonDown() && _playerState.CurrentState == State.OUTSIDE)
            {
                // Random animation parameters
                var index = (byte)Random.Range(0, _animationCount);

                PlayHitAnimation(index);
                AttackServerRpc(index);
            }

            // Path following
            if (_input.GetKeyDown(KeyCode.Q)) PathFollowing();

            // Showing cursor
            if (_input.GetKeyDown(KeyCode.LeftControl))
                Cursor.lockState = CursorLockMode.None;
            else if (_input.GetKeyUp(KeyCode.LeftControl))
                Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // Synchronize position and rotation
            transform.SetPositionAndRotation(_netTransform.Value.Position, _netTransform.Value.Rotation);
        }
    }


    private void FixedUpdate()
    {
        if (IsOwner)
        {
            if (_playerState.CurrentState == State.OUTSIDE)
                Move();
            if (Cursor.lockState != CursorLockMode.None)
                Rotate();

            UpdateNetPositionAndRotation();
        }
    }

    private void Move()
    {
        var input = GetKeyInput();
        MoveBasedOnInput(input);
    }

    private Vector2 GetKeyInput()
    {
        return _input.GetKeyAxis();
    }

    private void MoveBasedOnInput(Vector2 input)
    {
        _rb.velocity = (transform.forward * input.y + transform.right * input.x).normalized * _walkingSpeed;
    }

    private void Rotate()
    {
        var input = GetMouseInput();
        RotateBaseOnInput(input);
        _cameraController.UpdateCameraPositionAndRotation(_cameraVerticalAngle);
    }

    private Vector2 GetMouseInput()
    {
        return _input.GetMouseAxis();
    }

    private void RotateBaseOnInput(Vector2 input)
    {
        // Left-right
        transform.Rotate(0, input.x * _mouseSensitivity, 0);

        // Up-down
        _cameraVerticalAngle -= input.y * _mouseSensitivity;
        _cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, _cameraMinAngle, _cameraMaxAngle);
    }

    private void UpdateNetPositionAndRotation()
    {
        _netTransform.Value = new NetworkTransform()
        {
            Position = transform.position,
            Rotation = transform.rotation
        };
    }

    private void PathFollowing()
    {
        if (_playerState.CurrentState == State.ON_PATH)
        {
            // Stop path following
            _follower.enabled = false;
            _playerState.CurrentState = State.OUTSIDE;
        }
        else if (_playerState.CurrentState == State.OUTSIDE)
        {
            // Start path following
            _follower.enabled = true;
            _follower.StartAtCurrentPosition();
            _follower.Speed = _pathFollowingSpeed;
            _playerState.CurrentState = State.ON_PATH;
        }
    }

    /// Tries to enter or leave nearby building.
    /// Returns true if nearby building was found
    public bool BaseInteraction()
    {
        if (!FindBaseEntrance(out var baseController)) return false;

        switch (_playerState.CurrentState)
        {
            case State.INSIDE:
                _playerState.CurrentState = State.OUTSIDE;
                LeaveBase(baseController);
                break;
            case State.OUTSIDE:
                _playerState.CurrentState = State.INSIDE;
                RemoveFromHostilePlayersServerRpc();
                LeaveMoney(baseController);
                EnterBase(baseController);
                break;
        }

        return true;
    }


    public bool FindBaseEntrance(out BaseController baseControllerOut)
    {
        baseControllerOut = null;

        // Check for nearby entrances
        Collider[] entrances = Physics.OverlapSphere(transform.TransformPoint(_entranceDetectionCenter), _entranceDetectionRadius, _entranceLayer);

        // No entraces nearby
        if (entrances.Length == 0) return false;

        Transform entranceTransform = entrances[0].transform;

        // Check if that's base entrance
        if (!entranceTransform.gameObject.TryGetComponent<BaseController>(out var baseController)) return false;

        // Check if base's team is the same as player's
        if (_teamController.Team != baseController.Team) return false;

        baseControllerOut = baseController;
        return true;
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
        Collider[] vaults = Physics.OverlapSphere(transform.TransformPoint(_entranceDetectionCenter), _entranceDetectionRadius, _vaultLayer);

        if (vaults.Length == 0) return; // No money to collect

        // Add money for player
        _money = Mathf.Clamp(_money + _moneyCollection, min: 0, max: _maxMoney);
        MainUIController.Instance.UpdateMoneyText(_money);
    }


    [ServerRpc]
    private void RemoveFromHostilePlayersServerRpc() => HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);


    [ServerRpc]
    private void AttackServerRpc(byte animationIndex)
    {
        // Find enemies nearby
        Collider[] enemiesSphere = Physics.OverlapSphere(transform.position, _attackRange, _enemyLayer);
        Collider[] enemiesBox = Physics.OverlapBox(_attackCenter.position, Vector3.one * _attackRange, transform.rotation);

        foreach (Collider other in Enumerable.Intersect(enemiesSphere, enemiesBox))
        {
            // Check if this is the same player
            if (gameObject == other.gameObject) continue;

            // Check if object is dead
            if (other.GetComponent<IDead>().IsDead()) continue;

            // Check if that's a player from the same team
            if (other.gameObject.TryGetComponent<TeamController>(out var otherTeamController)
                && otherTeamController.Team == _teamController.Team) continue;

            // Hit
            other.GetComponent<Mortal>().TakeDamage(_damage);

            // Alarm guards
            HostilePlayerManager.Instance.AddToHostilePlayers(transform);
            _playerState.RestartHostileTimer();
        }

        // Play hit animation
        PlayHitAnimationClientRpc(animationIndex);
    }


    [ClientRpc]
    private void PlayHitAnimationClientRpc(byte animationIndex)
    {
        if (!IsOwner) // Owner has already played this animation
        {
            PlayHitAnimation(animationIndex);
        }
    }


    private void PlayHitAnimation(byte index)
    {
        _animator.SetInteger("Index", index);
        _animator.SetTrigger("Play");
    }


    public void Disappear()
    {
        // Make player invisible
        foreach (var renderer in _renderers) renderer.enabled = false;
        _rb.detectCollisions = false;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
    }


    public void EnterBase(BaseController baseController)
    {
        Disappear();

        // Switch cameras
        _cam.GetComponent<Camera>().enabled = false;
        baseController.EnableEntranceCamera(true);
    }


    public void Appear()
    {
        // Make player visible again
        foreach (var renderer in _renderers) renderer.enabled = true;
        _rb.detectCollisions = true;
        _rb.constraints = default_constraints;
    }


    private void LeaveBase(BaseController baseController)
    {
        Appear();

        // Switch cameras
        _cam.GetComponent<Camera>().enabled = true;
        baseController.EnableEntranceCamera(false);

        SetLeavingPosition(baseController);
    }


    private void SetLeavingPosition(BaseController baseController)
    {
        // Position + rotation left-right
        var entranceTransform = baseController.EntranceTransform;
        var leaving_position = entranceTransform.position;
        leaving_position.y = _spawnHeight;
        transform.SetPositionAndRotation(leaving_position, entranceTransform.rotation);
    }


    public void Respawn()
    {
        _playerState.SetNewStateServerRpc(State.OUTSIDE);

        _playerHealth.RegainHealthServerRpc();
    }


    private void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        //var halfAttackRange = _attackRange / 2f;
        Gizmos.DrawWireCube(_attackCenter.position, _attackRange * 2 * Vector3.one);

        // Entrance detection sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.TransformPoint(_entranceDetectionCenter), _entranceDetectionRadius);
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
}
