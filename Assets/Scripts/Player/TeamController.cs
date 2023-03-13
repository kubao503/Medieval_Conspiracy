using Unity.Netcode;


public class TeamController : NetworkBehaviour
{
    public static TeamController LocalPlayerInstance;
    private readonly NetworkVariable<Team> _netTeam = new(value: Team.Total, writePerm: NetworkVariableWritePermission.Owner);


    public Team Team
    {
        get => _netTeam.Value == Team.Total ? throw new InvalidTeamException() : _netTeam.Value;
        set => _netTeam.Value = value;
    }


    public override void OnNetworkSpawn()
    {
        //Debug.Log("LocalPlayerInstance setting by " + gameObject.name);
        if (IsLocalPlayer) LocalPlayerInstance = this;

        base.OnNetworkSpawn();
    }

    public bool IsTeamMatching(Team team)
    {
        return this.Team == team;
    }

    public void EndGame(Team loosingTeam)
    {
        MainUIController.Instance.ShowGameEndText(loosingTeam != Team);
    }
}
