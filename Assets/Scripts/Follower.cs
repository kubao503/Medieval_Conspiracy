using PathCreation;
using Unity.Netcode;
using UnityEngine;

public class Follower : NetworkBehaviour
{
    public static PathCreator NpcPath = null;
    public float Offset = 0f;

    private readonly NetworkVariable<float> netSpeed = new();
    private readonly Quaternion rotationOffset = Quaternion.Euler(0f, 0f, 90f);
    private readonly Quaternion reversingRotation = Quaternion.Euler(0f, 180f, 0f);
    private float distanceTravelled = 0f;
    private PathCreator pathCreator;

    public float Distance
    {
        get => this.distanceTravelled;
        set => this.distanceTravelled = value;
    }

    public Vector3 Direction
    {
        get => this.pathCreator.path.GetDirectionAtDistance(this.distanceTravelled);
    }

    public float Speed
    {
        set => this.netSpeed.Value = value;
    }

    public float PathLength
    {
        get => this.pathCreator.path.length;
    }

    private void Awake()
    {
        if (NpcPath == null) NpcPath = GameObject.Find("NPC Path").GetComponent<PathCreator>();
        this.pathCreator = NpcPath;
    }

    public void StartAtGivenPosition(float offset, float distance)
    {
        this.Offset = offset;
        this.distanceTravelled = distance;
    }

    public void StartAtCurrentPosition()
    {
        var distanceAlongPath = this.pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        this.distanceTravelled = distanceAlongPath;

        SetInitRotation(distanceAlongPath);
        SetInitOffset(distanceAlongPath);
    }

    private void SetInitRotation(float distanceAlongPath)
    {
        transform.rotation = this.pathCreator.path.GetRotationAtDistance(distanceAlongPath) * this.rotationOffset;
    }

    private void SetInitOffset(float distanceAlongPath)
    {
        var closestPoint = this.pathCreator.path.GetPointAtDistance(distanceAlongPath);
        var offsetVector = transform.position - closestPoint;
        var localOffsetVector = transform.InverseTransformDirection(offsetVector);
        this.Offset = localOffsetVector.x;
    }

    private void FixedUpdate()
    {
        UpdateDistanceTravelled();

        var position = GetPosition();
        var rotation = GetRotation();

        transform.SetPositionAndRotation(position, rotation);
    }

    private void UpdateDistanceTravelled()
    {
        this.distanceTravelled = Mathf.Repeat(this.distanceTravelled + this.netSpeed.Value, this.pathCreator.path.length);
    }

    private Vector3 GetPosition()
    {
        var positionAlongPath = this.pathCreator.path.GetPointAtDistance(this.distanceTravelled);
        var offsetVector = this.Offset * transform.right;
        if (IsReversed())
            offsetVector = GetOppositeVector(offsetVector);

        return positionAlongPath + offsetVector + Vector3.up;
    }

    private Vector3 GetOppositeVector(Vector3 vector)
    {
        return -vector;
    }

    private Quaternion GetRotation()
    {
        var rotation = this.pathCreator.path.GetRotationAtDistance(this.distanceTravelled) * this.rotationOffset;
        if (IsReversed())
            rotation = Rotate180Degrees(rotation);
        return rotation;
    }

    private bool IsReversed()
    {
        return this.netSpeed.Value < 0f;
    }

    private Quaternion Rotate180Degrees(Quaternion rotation)
    {
        return this.reversingRotation * rotation;
    }

    // Set speed value while continuing to walk in the same direction
    public void SetSpeed(float speed)
    {
        this.netSpeed.Value = speed * Mathf.Sign(this.netSpeed.Value);
    }
}
