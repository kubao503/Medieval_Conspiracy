using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;


public class LobbyUI : NetworkBehaviour
{
    public static LobbyUI Instance;

    [SerializeField] internal Image LoadingWheel;
    [SerializeField] internal float LoadingWheelRotationSpeed = 1f;
    [SerializeField] internal int MaxCodeLength;

    [System.NonSerialized] internal string TmpNick = "";
    [System.NonSerialized] internal string JoinCode = "";
    [System.NonSerialized] internal LobbyState CurrentState;

    public void Connected()
    {
        CurrentState.Connected();
    }

    public void Disconnected()
    {
        CurrentState.Disconnected();
    }

    public void SetLoadingWheelActive(bool state)
    {
        LoadingWheel.enabled = state;
    }

    private void Awake()
    {
        Instance = this;
        CurrentState = new NotConnectedUI(this);
    }

    private void FixedUpdate()
    {
        CurrentState.FixedUpdate();
    }

    private void OnGUI()
    {
        CurrentState.OnGUI();
    }
}
