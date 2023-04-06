using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;


public class TestPlayerMover
{
    private GameObject _player;
    private PlayerMover _playerMover;

    [UnityTest]
    public IEnumerator TestForwardMovement()
    {
        GivenPlayerWithPlayerMover();

        WhenKeyAxisInputEqual(new(0f, 1f));
        yield return new WaitForSeconds(.1f);

        ThenPlayerShouldMoveForward();
    }

    [UnityTest]
    public IEnumerator TestLeftRightRotation()
    {
        GivenPlayerWithPlayerMover();

        WhenMouseAxisInputEqual(new(1f, 0f));
        yield return new WaitForSeconds(.1f);

        ThenPlayerShouldRotateLeftRight();
    }

    [UnityTest]
    public IEnumerator TestUpDownRotation()
    {
        GivenPlayerWithPlayerMover();

        WhenMouseAxisInputEqual(new(0f, 1f));
        yield return new WaitForSeconds(.1f);

        ThenCameraShouldRotateUpDown();
    }

    private void GivenPlayerWithPlayerMover()
    {
        CreatePlayer();
        AddComponentsToPlayer();
    }

    private void CreatePlayer()
    {
        _player = new GameObject("Player");
        _player.transform.position = Vector3.zero;
        _player.transform.rotation = Quaternion.identity;
    }

    private void AddComponentsToPlayer()
    {
        _player.AddComponent<FakeCameraMover>();
        _player.AddComponent<Rigidbody>().useGravity = false;

        var playerState = _player.AddComponent<FakePlayerState>();
        playerState.CurrentState = PlayerState.State.Outside;
        Assert.True(playerState.CurrentState == PlayerState.State.Outside);

        _playerMover = _player.AddComponent<PlayerMover>();
    }

    private void WhenKeyAxisInputEqual(Vector2 keyAxisInput)
    {
        _playerMover.SetTestParameters( new FakeInputAdapter(
            keyAxisInput,
            Vector2.zero,
            true
        ));
    }

    private void WhenMouseAxisInputEqual(Vector2 mouseAxisInput)
    {
        _playerMover.SetTestParameters( new FakeInputAdapter(
            Vector2.zero,
            mouseAxisInput,
            true
        ));
    }

    private void ThenPlayerShouldMoveForward()
    {
        Assert.AreEqual(.0, _player.transform.position.x);
        Assert.AreEqual(.0, _player.transform.position.y);
        Assert.Positive(_player.transform.position.z);
    }

    private void ThenPlayerShouldRotateLeftRight()
    {
        var angleDifference = Quaternion.Angle(_player.transform.rotation, Quaternion.identity);
        Assert.Positive(angleDifference);
    }

    private void ThenCameraShouldRotateUpDown()
    {
        Assert.NotZero(_playerMover.CameraVerticalAngle);
    }
}
