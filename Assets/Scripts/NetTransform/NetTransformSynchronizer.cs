using Unity.Netcode;


public class NetTransformSynchronizer : NetworkBehaviour
{
    private readonly NetworkVariable<NetTransform> _netTransform = new(
        writePerm: NetworkVariableWritePermission.Owner);
    private HealthController _healthController;

    private void Awake()
    {
        _healthController = GetComponent<HealthController>();
    }

    private void Update()
    {
        if (IsOwner)
            SetNetTransformBasedOnLocalTransform();
        else if (IsAlive())
            SetLocalTransformBasedOnNetTransform();
    }

    private void SetNetTransformBasedOnLocalTransform()
    {
        _netTransform.Value = new NetTransform()
        {
            Position = transform.position,
            Rotation = transform.rotation
        };
    }

    private bool IsAlive()
    {
        return !_healthController.IsDead;
    }

    private void SetLocalTransformBasedOnNetTransform()
    {
        transform.SetPositionAndRotation(
            _netTransform.Value.Position,
            _netTransform.Value.Rotation);
    }
}
