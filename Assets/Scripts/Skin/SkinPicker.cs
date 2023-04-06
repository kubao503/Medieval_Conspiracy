using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using ElementType = UnityEngine.Renderer;


public class SkinPicker : NetworkBehaviour
{
    [SerializeField] private List<ElementType> _hatElements = new();
    [SerializeField] private List<ElementType> _shirtElements = new();
    [SerializeField] private List<ElementType> _pantsElements = new();
    [SerializeField] private List<ElementType> _skinElements = new();
    private readonly NetworkVariable<NetworkSkin> _netSkin = new();

    public void SetNetSkin()
    {
        var skin = SkinManager.Instance.GetRandomSkin();
        _netSkin.Value = skin;
    }

    public override void OnNetworkSpawn()
    {
        _netSkin.OnValueChanged += SetSkinBasedOnNetSkin;
    }

    private void SetSkinBasedOnNetSkin(NetworkSkin _, NetworkSkin newSkin)
    {
        var skin = _netSkin.Value;

        foreach (var hat in _hatElements)
            hat.material.color = skin.Hat.Color;

        foreach (var shirt in _shirtElements)
            shirt.material.color = skin.Shirt.Color;

        foreach (var pants in _pantsElements)
            pants.material.color = skin.Pants.Color;

        foreach (var skinColor in _skinElements)
            skinColor.material.color = skin.SkinColor.Color;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsServer)
            SkinManager.Instance.ReturnSkin(_netSkin.Value);
    }
}
