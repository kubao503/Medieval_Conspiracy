using UnityEngine;


[CreateAssetMenu]
public class FollowerParameters : ScriptableObject
{
    public readonly Quaternion RotationOffset = Quaternion.Euler(0f, 0f, 90f);
    public readonly Quaternion ReversingRotation = Quaternion.Euler(0f, 180f, 0f);

    [SerializeField] private float _minSpeed = 1f;
    [SerializeField] private float _maxSpeed = 1f;
    [SerializeField] private float _offsetRange = 1f;

    [Header("Resident")]
    [SerializeField] private float _runSpeed = 1f;

    public float MinSpeed => _minSpeed;
    public float MaxSpeed => _maxSpeed;
    public float OffsetRange => _offsetRange;
    public float RunSpeed => _runSpeed;
}
