using UnityEngine;


interface ICameraMover
{
    void UpdateCameraPositionAndRotation(float cameraVerticalAngle);
}


public class CameraMover : MonoBehaviour, ICameraMover
{
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _cameraHinge;
    [SerializeField] private LayerMask _ignoreLayers;
    [SerializeField] private float _maxDistance;
    [SerializeField] private float _minDistanceFromObstacle;
    private IRaycast _raycastAdapter = new RaycastAdapter();
    private const float _minAngle = -1.5707963f;
    private const float _maxAngle = 1.5707963f;

    public static float MinAngle => _minAngle;
    public static float MaxAngle => _maxAngle;

    public void SetTestParameters(
        Transform camera,
        Transform cameraHinge,
        float maxDistance,
        float minDistanceFromObstacle,
        IRaycast raycastAdapter)
    {
        this._camera = camera;
        this._cameraHinge = cameraHinge;
        this._maxDistance = maxDistance;
        this._minDistanceFromObstacle = minDistanceFromObstacle;
        this._raycastAdapter = raycastAdapter;
    }

    public void UpdateCameraPositionAndRotation(float cameraVerticalAngle)
    {
        var cameraDirection = GetCameraDirection(cameraVerticalAngle);
        SendRayInGivenDirection(cameraDirection);
        SetCameraPosition(cameraDirection);
        SetCameraRotation(cameraVerticalAngle);
    }

    private Vector3 GetCameraDirection(float cameraVerticalAngle)
    {
        var horizontalAngles = Quaternion.FromToRotation(Vector3.back, -transform.forward).eulerAngles.y;
        var horizontalRotation = Quaternion.Euler(0f, horizontalAngles, 0f);
        var verticalRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f);
        return horizontalRotation * verticalRotation * Vector3.back;
    }

    private void SendRayInGivenDirection(Vector3 direction)
    {
        _raycastAdapter.Raycast(_cameraHinge.position, direction, _maxDistance, _ignoreLayers);
    }

    private void SetCameraPosition(Vector3 cameraDirection)
    {
        var rayTrimDueToObstacleHit = GetRayTrimDueToObstacleHit(cameraDirection);

        _camera.position = _cameraHinge.position + cameraDirection
            * Mathf.Max(0f, _raycastAdapter.Distance - rayTrimDueToObstacleHit);
    }

    private void SetCameraRotation(float cameraVerticalAngle)
    {
        _camera.LookAt(_cameraHinge);
    }

    private float GetRayTrimDueToObstacleHit(Vector3 cameraDirection)
    {
        if (ObstacleNotHit())
            return 0f;

        var hitAngleInDegrees = Vector3.Angle(-cameraDirection, _raycastAdapter.SurfaceNormal);
        var hitAngleInRadians = hitAngleInDegrees * Mathf.Deg2Rad;
        hitAngleInRadians = LimitAngleInRadians(hitAngleInRadians);
        return _minDistanceFromObstacle / Mathf.Cos(hitAngleInRadians);
    }

    private float LimitAngleInRadians(float angle)
    {
        return Mathf.Clamp(angle, _minAngle, _maxAngle);
    }

    private bool ObstacleNotHit()
    {
        return !_raycastAdapter.ObstacleHit;
    }
}


public class FakeCameraMover : MonoBehaviour, ICameraMover
{
    public void UpdateCameraPositionAndRotation(float cameraVerticalAngle) { }
}
