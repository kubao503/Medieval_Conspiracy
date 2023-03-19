using Unity.Netcode;


public class TeamController : NetworkBehaviour
{
    public static TeamController LocalPlayerInstance;

    private readonly NetworkVariable<Team> _netTeam = new(Team.Total);
    private PlayerState _playerState;

    public Team Team
    {
        get => _netTeam.Value == Team.Total ? throw new InvalidTeamException() : _netTeam.Value;
        set => _netTeam.Value = value;
    }

    public bool IsTeamMatching(Team team)
    {
        return this.Team == team;
    }

    public void EndGame(Team loosingTeam)
    {
        MainUIController.Instance.ShowGameEndText(loosingTeam != Team);
    }

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayerInstance = this;
            _netTeam.OnValueChanged += TeamUpdated;
        }

        base.OnNetworkSpawn();
    }

    private void TeamUpdated(Team oldTeam, Team newTeam)
    {
        if (newTeam != Team.Total)
            _playerState.TeamSetServerRpc();
    }
}
