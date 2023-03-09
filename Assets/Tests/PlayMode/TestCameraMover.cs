using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestCameraMover
{
    GameObject _player;
    GameObject _camera;
    GameObject _cameraHinge;
    CameraMover _cameraController;
    const float _obstacleDistance = 10f;
    const float _minDistanceFromObstacle = .5f;
    const float _delta = 0.01f;

    [Test]
    public void TestUpdateCameraPositionAndRotationWithZeroAngle()
    {
        GivenThereIsObstacleAtDistance(_obstacleDistance);

        WhenVerticalAngleEquals(0f);

        var cameraDistance = _obstacleDistance - _minDistanceFromObstacle;
        var expectedCameraPosition = new Vector3(0f, 0f, -cameraDistance);
        ThenCameraShouldBeAt(expectedCameraPosition);
        ThenCameraShouldLookAt(Quaternion.Euler(0f, 0f, 0f));
    }

    [Test]
    public void TestUpdateCameraPositionAndRotation45Degrees()
    {
        GivenThereIsObstacleAtDistance(_obstacleDistance * Mathf.Sqrt(2));

        WhenVerticalAngleEquals(45f);

        var obstacleDistance = _obstacleDistance - _minDistanceFromObstacle;
        var expectedCameraPosition = new Vector3(0f, obstacleDistance, -obstacleDistance);
        ThenCameraShouldBeAt(expectedCameraPosition);
        ThenCameraShouldLookAt(Quaternion.Euler(45f, 0f, 0f));
    }

    [Test]
    public void TestUpdateCameraPositionAndRotation90Degrees()
    {
        GivenThereIsObstacleAtDistance(_obstacleDistance);

        WhenVerticalAngleEquals(90f);

        var expectedCameraPosition = new Vector3(0f, 0f, 0f);
        ThenCameraShouldBeAt(expectedCameraPosition);
    }

    [Test]
    public void TestUpdateCameraPositionAndRotationMinus90Degrees()
    {
        GivenThereIsObstacleAtDistance(_obstacleDistance);

        WhenVerticalAngleEquals(-90f);

        var expectedCameraPosition = new Vector3(0f, 0f, 0f);
        ThenCameraShouldBeAt(expectedCameraPosition);
    }

    [Test]
    public void TestMinAndMaxAngle()
    {
        Assert.Positive(Mathf.Cos(CameraMover.MinAngle));
        Assert.Positive(Mathf.Cos(CameraMover.MaxAngle));
    }

    [Test]
    public void TestUpdateCameraPositionAndRotationFromTheRight()
    {
        GivenThereIsObstacleAtDistance(_obstacleDistance);
        AndPlayerIsLookingFromTheRight();

        WhenVerticalAngleEquals(0f);

        ThenCameraShouldLookAt(Quaternion.Euler(0f, -90f, 0f));
    }

    private void GivenThereIsObstacleAtDistance(float distance)
    {
        CreateObjects();
        var raycastAdapter = new FakeRaycastAdapter(distance, Vector3.forward, true);
        AddCameraControllerToPlayer(raycastAdapter);
    }

    private void AndPlayerIsLookingFromTheRight()
    {
        _player.transform.Rotate(0f, -90f, 0f);
    }

    private void CreateObjects()
    {
        _player = new GameObject("Player");
        _camera = new GameObject("Camera");
        _cameraHinge = new GameObject("Hinge");

        _player.transform.position = Vector3.zero;
        _camera.transform.position = Vector3.zero;
        _cameraHinge.transform.position = Vector3.zero;

        _camera.transform.SetParent(_player.transform);
    }

    private void AddCameraControllerToPlayer(IRaycast raycastAdapter)
    {
        _cameraController = _player.AddComponent<CameraMover>();
        _cameraController.SetTestParameters(
            _camera.transform,
            _cameraHinge.transform,
            _obstacleDistance,
            _minDistanceFromObstacle,
            raycastAdapter);
    }

    private void WhenVerticalAngleEquals(float value)
    {
        _cameraController.UpdateCameraPositionAndRotation(value);
    }

    private void ThenCameraShouldBeAt(Vector3 expected)
    {
        AssertAreEqual(expected, _camera.transform.position);
    }

    private void ThenCameraShouldLookAt(Quaternion expected)
    {
        var angleDifference = Quaternion.Angle(expected, _camera.transform.rotation);
        Assert.Less(angleDifference, _delta);
    }

    private void AssertAreEqual(Vector3 expected, Vector3 actual)
    {
        Assert.AreEqual(expected.x, actual.x, _delta);
        Assert.AreEqual(expected.y, actual.y, _delta);
        Assert.AreEqual(expected.z, actual.z, _delta);
    }
}
