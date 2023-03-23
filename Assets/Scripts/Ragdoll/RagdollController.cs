using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private Layer _deadLayer;
    private Rigidbody _tmpRigidBody;
    private int _defaultLayer;
    private readonly Vector3 _torque = new(.2f, .1f, .2f);
    private const float _spawnHeight = 1f;

    public void FallDown()
    {
        AddTemporaryRigidbodyIfMissing();

        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.constraints = RigidbodyConstraints.None;
        rigidBody.AddTorque(_torque, ForceMode.VelocityChange);

        gameObject.layer = GetLayerFromLayerMask(_deadLayer.Value);
    }

    private void AddTemporaryRigidbodyIfMissing()
    {
        if (IsRigidbodyMissing())
            _tmpRigidBody = gameObject.AddComponent<Rigidbody>();
    }

    private bool IsRigidbodyMissing()
    {
        return null == GetComponent<Rigidbody>();
    }

    public void StandUp()
    {
        SetTransformToStanding();
        RemoveTemporaryRigidbody();
        //gameObject.layer = GetLayerFromLayerMask(_defaultLayer);
        gameObject.layer = _defaultLayer;
    }

    private void SetTransformToStanding()
    {
        transform.rotation = Quaternion.identity;

        var standingPosition = transform.position;
        standingPosition.y = _spawnHeight;
        transform.position = standingPosition;
    }

    private void RemoveTemporaryRigidbody()
    {
        Destroy(_tmpRigidBody);
    }

    private int GetLayerFromLayerMask(LayerMask layerMask)
    {
        return (int)Mathf.Log(layerMask, 2);
    }

    private void Awake()
    {
        _defaultLayer = gameObject.layer;
    }
}
