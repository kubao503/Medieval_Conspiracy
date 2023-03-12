using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System;

using State = PlayerState.State;

public class PlayerController : NetworkBehaviour
{
    private readonly NetworkVariable<NetworkTransform> _netTransform = new(writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private LayerMask _vaultLayer;
    [SerializeField] private float _pathFollowingSpeed;
    [SerializeField] private float _attackRange;
    [SerializeField] private Transform _attackCenter;
    [SerializeField] private int _damage;
    [SerializeField] private int _animationCount;
    private AudioListener _audioListener;
    private PlayerHealth _playerHealth;
    private PlayerState _playerState;
    private PlayerHostility _playerHostility;
    private TeamController _teamController;
    private BaseInteractions _baseInteractions;
    private Rigidbody _rb;
    private Renderer[] _renderers;
    private Animator _animator;
    private Follower _follower;
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
        _renderers = GetComponentsInChildren<Renderer>();
        _animator = GetComponent<Animator>();
        _audioListener = GetComponent<AudioListener>();
        _playerHealth = GetComponent<PlayerHealth>();
        _playerState = GetComponent<PlayerState>();
        _playerHostility = GetComponent<PlayerHostility>();
        _teamController = GetComponent<TeamController>();
        _baseInteractions = GetComponent<BaseInteractions>();
        _follower = GetComponent<Follower>();
    }

    private void SubscribeToEvents()
    {
        _playerState.StateUpdated += StateUpdated;
        _playerHealth.Died += Die;
    }

    private void StateUpdated(object sender, EventArgs e)
    {
        switch (_playerState.CurrentState)
        {
            case State.OUTSIDE:
                LeaveBase();
                break;
            case State.INSIDE:
                    EnterBase();
                break;
            case State.DEAD:
                Disappear();
                break;
        }
    }

    private void Die(object sender, EventArgs e)
    {
        HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);
        _playerHostility.StopHostileTimer();

        Disappear();
        _playerState.CurrentState = PlayerState.State.DEAD;

        TeamManager.Instance.DeadPlayerUpdate(GetComponent<TeamController>().Team, OwnerClientId);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            //MainUIController.Instance.SubscribeToRespawnClick(Respawn);
        }
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
            if (_input.GetKeyDown(KeyCode.Escape)) Quit();

            // Entering entrance
            if (_input.GetKeyDown(KeyCode.E)) CollectMoney();

            // Fight
            if (_input.GetLeftMouseButtonDown() && _playerState.CurrentState == State.OUTSIDE)
            {
                // Random animation parameters
                var index = (byte)UnityEngine.Random.Range(0, _animationCount);

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
            if (other.GetComponent<HealthController>().IsDead) continue;

            // Check if that's a player from the same team
            if (other.gameObject.TryGetComponent<TeamController>(out var otherTeamController)
                && otherTeamController.Team == _teamController.Team) continue;

            // Hit
            other.GetComponent<HealthController>().TakeDamage(_damage);

            // Alarm guards
            HostilePlayerManager.Instance.AddToHostilePlayers(transform);
            _playerHostility.RestartHostileTimer();
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


    public void Respawn()
    {
        _playerState.CurrentState = State.OUTSIDE;

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
        var insideBase = _playerState.CurrentState == State.INSIDE;
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
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        //var halfAttackRange = _attackRange / 2f;
        Gizmos.DrawWireCube(_attackCenter.position, _attackRange * 2 * Vector3.one);

        // Entrance detection sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.TransformPoint(_moneyCollectionCenter), _moneyCollectionRadius);
    }
}
