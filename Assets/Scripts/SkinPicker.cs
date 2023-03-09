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


    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GetAvailableSkin();
        }
        else
        {
            // Get skin color
            _netSkin.OnValueChanged += SetSkin;
            SetSkin(_netSkin.Value, _netSkin.Value);
        }
    }


    public override void OnDestroy()
    {
        // Return skin back to SkinManager
        if (IsServer) SkinManager.Instance.ReturnSkin(_netSkin.Value);

        base.OnDestroy();
    }


    private void GetAvailableSkin()
    {
        var skin = SkinManager.Instance.GetAvailableSkin();
        SetSkin(skin);

        // Synchronize
        _netSkin.Value = new(skin);
    }


    private void SetSkin(NetworkSkin _, NetworkSkin skin) => SetSkin(skin);


    // Server-side
    private void SetSkin(in NetworkSkin skin)
    {
        foreach (var hat in _hatElements) hat.material.color = skin.Hat.Color;
        foreach (var shirt in _shirtElements) shirt.material.color = skin.Shirt.Color;
        foreach (var pants in _pantsElements) pants.material.color = skin.Pants.Color;
        foreach (var skinColor in _skinElements) skinColor  .material.color = skin.SkinColor.Color;
    }
}
