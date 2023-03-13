using UnityEngine;
using Unity.Netcode;

public class BaseController : NetworkBehaviour
{
    [SerializeField] private Camera _camera;

    private readonly NetworkVariable<Team> _netTeam = new(value: Team.Total);
    private int _money = 0;

    public Team Team
    {
        get => _netTeam.Value;
        set => _netTeam.Value = value;
    }


    public void EnableEntranceCamera(bool enabled) => _camera.enabled = enabled;


    public Transform EntranceTransform => transform;


    public void LeaveMoney(int money) => _money += money;
}
