using UnityEngine;
using System.Collections;

public class GateOpener : MonoBehaviour
{
    [SerializeField] private Transform _gate;
    private const float _openHeight = 3.85f;
    private const float _closedHeight = 0f;


    private void Start()
    {
        ChangeState(true);
    }


    public void ChangeState(bool open)
    {
        var position = _gate.position;
        position.y = open ? _openHeight : _closedHeight;
        _gate.position = position;
    }
}
