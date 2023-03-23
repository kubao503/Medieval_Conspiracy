using UnityEngine;


[CreateAssetMenu]
public class Layer : ScriptableObject
{
    [SerializeField] private LayerMask _layer;

    public LayerMask Value => _layer;
}
