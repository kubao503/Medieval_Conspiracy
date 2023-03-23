using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private Layer _deadLayer;
    private Rigidbody _rigidBody;
    private LayerMask _defaultLayer;
    private readonly Vector3 _torque = new(.2f, .1f, .2f);

    public void FallDown()
    {
        EnsureRigidbodyIsAttached();
        _rigidBody.constraints = RigidbodyConstraints.None;
        _rigidBody.AddTorque(_torque, ForceMode.VelocityChange);
        gameObject.layer = GetLayerFromLayerMask(_deadLayer.Value);
    }

    private void EnsureRigidbodyIsAttached()
    {
        _rigidBody = GetComponent<Rigidbody>();
        if (_rigidBody == null)
            _rigidBody = gameObject.AddComponent<Rigidbody>();
    }

    public void SetBackToDefaultLayer()
    {
        gameObject.layer = GetLayerFromLayerMask(_defaultLayer);
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
