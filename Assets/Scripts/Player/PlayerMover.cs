using UnityEngine;


public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float _walkingSpeed;
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _cameraMaxAngle;
    [SerializeField] private float _cameraMinAngle;
    private IInput _input = InputAdapter.Instance;
    private Rigidbody _rb;
    private CameraMover _cameraMover;
    private float _cameraVerticalAngle = 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _cameraMover = GetComponent<CameraMover>();
    }

    private void FixedUpdate()
    {
        Move();
        if (Cursor.lockState != CursorLockMode.None)
            Rotate();
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
        LeftRightRotationBasedOnInput(input);
        UpDownRotationBasedOnInput(input);
        _cameraMover.UpdateCameraPositionAndRotation(_cameraVerticalAngle);
    }

    private Vector2 GetMouseInput()
    {
        return _input.GetMouseAxis();
    }

    private void LeftRightRotationBasedOnInput(Vector2 input)
    {
        transform.Rotate(0, input.x * _mouseSensitivity, 0);
    }

    private void UpDownRotationBasedOnInput(Vector2 input)
    {
        _cameraVerticalAngle -= input.y * _mouseSensitivity;
        _cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, _cameraMinAngle, _cameraMaxAngle);
    }
}
