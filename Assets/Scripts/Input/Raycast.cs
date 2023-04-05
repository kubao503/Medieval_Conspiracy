using UnityEngine;


public interface IRaycast
{
    public void Raycast(
        Vector3 cameraHingePosition,
        Vector3 cameraDirection,
        float maxDistance,
        LayerMask obstacleLayer);

    float Distance { get; }
    Vector3 SurfaceNormal { get; }
    bool ObstacleHit { get; }
}


public class RaycastAdapter : IRaycast
{
    private RaycastHit _hitInfo = new();
    private bool _obstacleHit;

    public float Distance => _hitInfo.distance;
    public Vector3 SurfaceNormal => _hitInfo.normal;
    public bool ObstacleHit => _obstacleHit;

    public void Raycast(
        Vector3 cameraHingePosition,
        Vector3 cameraDirection,
        float maxDistance,
        LayerMask ignoreLayers)
    {
        _obstacleHit = Physics.Raycast(
            cameraHingePosition,
            cameraDirection,
            out _hitInfo,
            maxDistance,
            ~ignoreLayers);

        if (!_obstacleHit)
            _hitInfo.distance = maxDistance;
    }
}


public class FakeRaycastAdapter : IRaycast
{
    public FakeRaycastAdapter(float distance, Vector3 surfaceNormal, bool obstacleHit)
    {
        Distance = distance;
        SurfaceNormal = surfaceNormal;
        ObstacleHit = obstacleHit;
    }

    public void Raycast(
        Vector3 cameraHingePosition,
        Vector3 cameraDirection,
        float maxDistance,
        LayerMask obstacleLayer) { }

    public float Distance { get; set; }
    public Vector3 SurfaceNormal { get; set; }
    public bool ObstacleHit { get; set; }
}
