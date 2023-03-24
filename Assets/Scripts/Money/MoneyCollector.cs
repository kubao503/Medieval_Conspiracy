using System;
using UnityEngine;
using Unity.Netcode;

using MoneyType = System.Int32;

public class MoneyCollector : NetworkBehaviour
{
    // TODO: Add MoneyCollected event

    [SerializeField] private LayerMask _vaultLayer;
    private IInput _input = InputAdapter.Instance;
    private MoneyType _amount = 0;
    private readonly Vector3 _collectionCenter = new(0f, -.5f, 0f);
    private const float _collectionRadius = .5f;
    private KeyCode _collectionKey = KeyCode.E;
    private const MoneyType _collectionAmount = 1;
    private const MoneyType _maxAmount = 5;

    private void Update()
    {
        if (IsOwner && _input.GetKeyDown(_collectionKey))
            TryCollectMoney();
    }

    private void TryCollectMoney()
    {
        if (AreThereNearbyVaults())
            CollectMoney();
    }

    private bool AreThereNearbyVaults()
    {
        Collider[] vaults = GetNearbyVaults();
        return vaults.Length != 0;
    }

    private Collider[] GetNearbyVaults()
    {
        var localCollectionCenter = transform.TransformPoint(_collectionCenter);
        return Physics.OverlapSphere(localCollectionCenter, _collectionRadius, _vaultLayer);
    }

    private void CollectMoney()
    {
        IncreaseMoneyAmount();
        MainUIController.Instance.UpdateMoneyText(_amount);
    }

    private void IncreaseMoneyAmount()
    {
        _amount = Mathf.Clamp(
            _amount + _collectionAmount,
            min: 0,
            max: _maxAmount);
    }

    // TODO: Remove if unused
    private void LeaveMoney(BaseController baseController)
    {
        baseController.LeaveMoney(_amount);
        _amount = 0;
        MainUIController.Instance.UpdateMoneyText(_amount);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(
            transform.TransformPoint(_collectionCenter),
            _collectionRadius);
    }
}
