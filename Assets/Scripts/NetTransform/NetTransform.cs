using UnityEngine;
using Unity.Netcode;


internal struct NetTransform : INetworkSerializable
{
    private float _xPosition, _zPosition;
    private short _rotation;

    public Vector3 Position
    {
        readonly get => new(_xPosition, 1f, _zPosition);
        set
        {
            _xPosition = value.x;
            _zPosition = value.z;
        }
    }

    public Quaternion Rotation
    {
        readonly get => Quaternion.Euler(0f, _rotation, 0f);
        set => _rotation = (short)Mathf.Round(value.eulerAngles.y);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _xPosition);
        serializer.SerializeValue(ref _zPosition);
        serializer.SerializeValue(ref _rotation);
    }
}
