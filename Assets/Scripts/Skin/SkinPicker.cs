using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using ElementType = UnityEngine.Renderer;


public class SkinPicker : NetworkBehaviour
{
    private readonly NetworkVariable<NetworkSkin> _netSkin = new();
    [SerializeField] private List<ElementType> _hatElements = new();
    [SerializeField] private List<ElementType> _shirtElements = new();
    [SerializeField] private List<ElementType> _pantsElements = new();
    [SerializeField] private List<ElementType> _skinElements = new();


    public override void OnNetworkSpawn()
    {
        _netSkin.OnValueChanged += SetSkinBasedOnNetSkin;
    }


    public override void OnDestroy()
    {
        // Return skin back to SkinManager
        if (IsServer) SkinManager.Instance.ReturnSkin(_netSkin.Value);

        base.OnDestroy();
    }


    public void SetNetSkin()
    {
        var skin = SkinManager.Instance.GetAvailableSkin();
        _netSkin.Value = new(skin);
    }

    private void SetSkinBasedOnNetSkin(NetworkSkin _, NetworkSkin newSkin)
    {
        var skin = _netSkin.Value;
        foreach (var hat in _hatElements) hat.material.color = skin.Hat.Color;
        foreach (var shirt in _shirtElements) shirt.material.color = skin.Shirt.Color;
        foreach (var pants in _pantsElements) pants.material.color = skin.Pants.Color;
        foreach (var skinColor in _skinElements) skinColor  .material.color = skin.SkinColor.Color;
    }
}
