using UnityEngine;
using Unity.Netcode;


public abstract class LobbyState
{
    protected LobbyUI _lobbyUI;

    public LobbyState(LobbyUI lobbyUI)
    {
        this._lobbyUI = lobbyUI;
        _lobbyUI.SetLoadingWheelActive(false);
    }

    public abstract void OnGUI();
    public abstract void Connected();
    public abstract void Disconnected();
    public abstract void FixedUpdate();
}


public class NotConnectedUI : LobbyState
{
    public NotConnectedUI(LobbyUI lobbyUI) : base(lobbyUI) { }

    public override void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0f, 0f, 200f, 200));

        _lobbyUI.JoinCode = GUILayout.TextField(_lobbyUI.JoinCode, _lobbyUI.MaxCodeLength);

        if (GUILayout.Button("Global host"))
        {
            _lobbyUI.CurrentState = new ConnectingUI(_lobbyUI);
            NetworkController.Instance.CreateGlobalGame();
        }
        if (GUILayout.Button("Global client"))
        {
            _lobbyUI.CurrentState = new ConnectingUI(_lobbyUI);
            NetworkController.Instance.JoinGlobalGame(_lobbyUI.JoinCode);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Local host"))
        {
            _lobbyUI.CurrentState = new ConnectingUI(_lobbyUI);
            NetworkController.Instance.CreateLocalGame();
        }
        if (GUILayout.Button("Local client"))
        {
            _lobbyUI.CurrentState = new ConnectingUI(_lobbyUI);
            NetworkController.Instance.JoinLocalGame();
        }

        GUILayout.EndArea();
    }

    public override void Connected() { }
    public override void Disconnected() { }
    public override void FixedUpdate() { }
}


public class ConnectingUI : LobbyState
{
    public ConnectingUI(LobbyUI lobbyUI) : base(lobbyUI)
    {
        _lobbyUI.SetLoadingWheelActive(true);
    }

    public override void OnGUI() { }

    public override void Connected()
    {
        _lobbyUI.CurrentState = new NickChoosingUI(_lobbyUI);
    }

    public override void Disconnected()
    {
        _lobbyUI.CurrentState = new NotConnectedUI(_lobbyUI);
    }

    public override void FixedUpdate()
    {
        DisplayLoadingWheel();
    }

    private void DisplayLoadingWheel()
    {
        _lobbyUI.LoadingWheel.rectTransform.Rotate(
            Vector3.forward,
            _lobbyUI.LoadingWheelRotationSpeed);
    }
}


class NickChoosingUI : LobbyState
{
    public NickChoosingUI(LobbyUI lobbyUI) : base(lobbyUI) { }

    public override void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0f, 0f, 200f, 200));

        GUILayout.Label("Your nick:");
        _lobbyUI.TmpNick = GUILayout.TextField(_lobbyUI.TmpNick);

        if (GUILayout.Button("Submit") && LobbyPlayerData.IsNickCorrect(_lobbyUI.TmpNick))
        {
            LobbyPlayerData.LocalPlayer.Nick = _lobbyUI.TmpNick;
            _lobbyUI.CurrentState = new TeamChoosingUI(_lobbyUI);
        }

        GUILayout.EndArea();
    }

    public override void Connected() { }

    public override void Disconnected()
    {
        _lobbyUI.CurrentState = new NotConnectedUI(_lobbyUI);
    }

    public override void FixedUpdate() { }
}

public class TeamChoosingUI : LobbyState
{
    public TeamChoosingUI(LobbyUI lobbyUI) : base(lobbyUI) { }

    public override void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0f, 0f, 200f, 200));

        // First column
        if (GUILayout.Button("Team A") && !LobbyPlayerData.LocalPlayer.Ready)
            LobbyPlayerData.LocalPlayer.Team = Team.A;

        foreach (var nick in LobbyPlayerDataManager.Instance.GetNicksOfPlayersFromTeam(Team.A))
            GUILayout.Label(nick);

        GUILayout.EndArea();

        // Second column
        GUILayout.BeginArea(new Rect(200f, 0f, 200f, 200));

        if (GUILayout.Button("Team B") && !LobbyPlayerData.LocalPlayer.Ready)
            LobbyPlayerData.LocalPlayer.Team = Team.B;

        foreach (var nick in LobbyPlayerDataManager.Instance.GetNicksOfPlayersFromTeam(Team.B))
            GUILayout.Label(nick);

        GUILayout.EndArea();

        // Ready button
        GUILayout.BeginArea(new Rect(0f, 200f, 200f, 200f));

        if (GUILayout.Button(LobbyPlayerData.LocalPlayer.Ready ? "Not Ready" : "Ready"))
            LobbyPlayerData.LocalPlayer.ToggleReady();

        // Start button
        if (IsServerAndAllPlayersAreReady() && GUILayout.Button("Start"))
            NetworkController.Instance.LoadGameScene();

        DisplayJoinCode();

        GUILayout.EndArea();
    }

    private bool IsServerAndAllPlayersAreReady()
    {
        return NetworkManager.Singleton.IsServer
               && LobbyPlayerDataManager.Instance.AreAllPlayersReady();
    }

    private void DisplayJoinCode()
    {
        if (_lobbyUI.JoinCode != string.Empty)
            GUILayout.Label("Join code: " + _lobbyUI.JoinCode);
    }

    public override void Connected() { }

    public override void Disconnected()
    {
        _lobbyUI.CurrentState = new NotConnectedUI(_lobbyUI);
    }

    public override void FixedUpdate() { }
}
