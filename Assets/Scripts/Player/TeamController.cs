using Unity.Netcode;


public class TeamController : NetworkBehaviour
{
    public static TeamController LocalPlayerInstance;
    private readonly NetworkVariable<Team> _netTeam = new(value: Team.TOTAL, writePerm: NetworkVariableWritePermission.Owner);


    public Team Team
    {
        get => _netTeam.Value == Team.TOTAL ? throw new InvalidTeamException() : _netTeam.Value;
        set => _netTeam.Value = value;
    }


    public override void OnNetworkSpawn()
    {
        //Debug.Log("LocalPlayerInstance setting by " + gameObject.name);
        if (IsLocalPlayer) LocalPlayerInstance = this;

        base.OnNetworkSpawn();
    }


    [ClientRpc]
    public void EndGameClientRpc(Team loosingTeam)
    {
        if (IsOwner) MainUIController.Instance.ShowGameEndText(loosingTeam != Team);
    }

    public bool IsTeamMatching(Team team)
    {
        return this.Team == team;
    }
}
