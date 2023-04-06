using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance;

    [SerializeField] private List<Material> _hatMaterials = new();
    [SerializeField] private List<Material> _shirtMaterials = new();
    [SerializeField] private List<Material> _pantsMaterials = new();
    [SerializeField] private List<Material> _skinColors = new();
    private readonly List<NetworkSkin> _availableSkins = new();

    public int SkinsLeft => _availableSkins.Count;

    public NetworkSkin GetRandomSkin()
    {
        if (_availableSkins.Count == 0)
            throw new OutOfSkinsException();

        return PopRandomSkin();
    }

    private NetworkSkin PopRandomSkin()
    {
        int index = Random.Range(0, _availableSkins.Count);
        var skin = _availableSkins[index];
        _availableSkins.RemoveAt(index);

        return skin;
    }

    public void ReturnSkin(NetworkSkin skin)
    {
        _availableSkins.Add(skin);
    }

    private void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Instance = this;
            GenerateSkins();
        }
        else
            Destroy(gameObject);
    }

    private void GenerateSkins()
    {
        if (_availableSkins.Count != 0)
            return;

        foreach (var hat in _hatMaterials)
            foreach (var shirt in _shirtMaterials)
                foreach (var pants in _pantsMaterials)
                    foreach (var skinColor in _skinColors)
                        _availableSkins.Add( new(hat.color, shirt.color, pants.color, skinColor.color) );
    }
}


public class OutOfSkinsException : UnityException { }


public struct NetworkColor : INetworkSerializable
{
    private byte _r, _g, _b;
    const byte _maxValue = byte.MaxValue;
    const float _alpha = 1f;

    public Color Color
    {
        readonly get
        {
            return new()
            {
                r = (float)_r / _maxValue,
                g = (float)_g / _maxValue,
                b = (float)_b / _maxValue,
                a = _alpha
            };
        }

        set
        {
            _r = (byte)Mathf.RoundToInt(value.r * _maxValue);
            _g = (byte)Mathf.RoundToInt(value.g * _maxValue);
            _b = (byte)Mathf.RoundToInt(value.b * _maxValue);
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _r);
        serializer.SerializeValue(ref _g);
        serializer.SerializeValue(ref _b);
    }
}


public struct NetworkSkin : INetworkSerializable
{
    private NetworkColor _hat, _shirt, _pants, _skinColor;

    public NetworkSkin(Color hat, Color shirt, Color pants, Color skinColor)
    {
        _hat = new() { Color = hat };
        _shirt = new() { Color = shirt };
        _pants = new() { Color = pants };
        _skinColor = new() { Color = skinColor };
    }

    public readonly NetworkColor Hat => _hat;
    public readonly NetworkColor Shirt => _shirt;
    public readonly NetworkColor Pants => _pants;
    public readonly NetworkColor SkinColor => _skinColor;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _hat);
        serializer.SerializeValue(ref _shirt);
        serializer.SerializeValue(ref _pants);
        serializer.SerializeValue(ref _skinColor);
    }
}
