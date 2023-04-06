using UnityEngine;
using Unity.Netcode;


[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : NetworkBehaviour
{
    [SerializeField] private float _walkingSpeed = 1f;
    [SerializeField] private float _mouseSensitivity = 1f;
    [SerializeField] private float _cameraMaxAngle = 90f;
    [SerializeField] private float _cameraMinAngle = -90f;
    private IInput _input = InputAdapter.Instance;
    private Rigidbody _rb;
    private ICameraMover _cameraMover;
    private IPlayerState _playerState;
    private float _cameraVerticalAngle = 0f;

    public float CameraVerticalAngle => _cameraVerticalAngle;

    public void SetTestParameters(IInput input)
    {
        this._input = input;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _cameraMover = GetComponent<ICameraMover>();
        _playerState = GetComponent<IPlayerState>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            this.enabled = false;
    }

    private void FixedUpdate()
    {
        if (_playerState.CurrentState == PlayerState.State.Outside)
            Move();
        if (IsPlayerAlive())
            Rotate();
        UpdateCameraPositionAndRotation();
    }

    private void Move()
    {
        var keyInput = _input.GetKeyAxis();
        MoveBasedOnInput(keyInput);
    }

    private void MoveBasedOnInput(Vector2 input)
    {
        _rb.velocity = (transform.forward * input.y + transform.right * input.x).normalized * _walkingSpeed;
    }

    bool IsPlayerAlive()
    {
        return _playerState.CurrentState != PlayerState.State.Dead
            && _playerState.CurrentState != PlayerState.State.Ragdoll;
    }

    private void Rotate()
    {
        var input = GetMouseInput();
        LeftRightRotationBasedOnInput(input);
        UpDownRotationBasedOnInput(input);
    }

    private void UpdateCameraPositionAndRotation()
    {
        _cameraMover.UpdateCameraPositionAndRotation(_cameraVerticalAngle);
    }

    private Vector2 GetMouseInput()
    {
        return _input.GetMouseAxis();
    }

    private void LeftRightRotationBasedOnInput(Vector2 input)
    {
        transform.Rotate(0f, input.x * _mouseSensitivity, 0f);
    }

    private void UpDownRotationBasedOnInput(Vector2 input)
    {
        _cameraVerticalAngle -= input.y * _mouseSensitivity;
        _cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, _cameraMinAngle, _cameraMaxAngle);
    }
}
